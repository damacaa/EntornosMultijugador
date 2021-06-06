using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

/// <summary>
/// Controlador de player que recoge los inputs y se los manda al servidor para poder pintar por pantalla
/// </summary>
public class PlayerController : NetworkBehaviour
{
    #region Variables

    [Header("Movement")] public List<AxleInfo> axleInfos;
    public float forwardMotorTorque = 100000;
    public float backwardMotorTorque = 50000;
    public float maxSteeringAngle = 15;
    public float engineBrake = 1e+12f;
    public float footBrake = 1e+24f;
    public float topSpeed = 200f;
    public float downForce = 100f;
    public float slipLimit = 0.2f;

    //Rotacion e Inputs
    private float CurrentRotation { get; set; }
    private float InputAcceleration { get; set; }
    private float InputSteering { get; set; }
    private float InputBrake { get; set; }

    private Rigidbody m_Rigidbody;
    private PlayerInfo _playerInfo;
    private UIManager _uiManager;
    private PolePositionManager _polePosition;
    
    [SyncVar(hook = nameof(UpdateSpeed))] private float m_CurrentSpeed = 0;
    private float m_SteerHelper = 0.8f;

    //Variable que indica si el localPlayer va marcha atras.
    public bool goingBackwards = false;
  
    private float BackwardsTimeout = 0;

    //Distancia a recorrer para acbar todas la vueltas del circuito
    public float distToFinish;
    public float DistToFinish
    {
        get { return distToFinish; }
        set
        {
            float d = value - distToFinish;
            float threshhold = 0.002f;
            goingBackwards = d < -threshhold && d > -100f;
            if (goingBackwards)
            {
                //Debug.Log(distToFinish + " --> " + value);
                BackwardsTimeout = 0.1f;//Tiempo que se mantiene en pantalla el aviso de marcha atrás
            }
            distToFinish = value;
        }
    }


    //Variable que indica si el localPlayer se ha chocado
    private bool crashed = false;
    public bool Crashed
    {
        get { return crashed; }
        set
        {
            if (OnHasCrashedEvent != null && crashed != value)
                OnHasCrashedEvent(value);

            crashed = value;
        }
    }

    public float Speed
    {
        get { return m_CurrentSpeed; }
        set
        {
            if (Math.Abs(m_CurrentSpeed - value) < float.Epsilon) return;
            m_CurrentSpeed = value;
        }
    }

    //Evento en caso de Chocar
    public delegate void OnHasCrashedDelegate(bool newVal);
    public event OnHasCrashedDelegate OnHasCrashedEvent;

    private _InputController _input; //call our own action controller

    #endregion Variables

    #region Unity Callbacks

    //Referencias
    public void Awake()
    {
        _input = new _InputController();
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManager>();
        if (_polePosition == null) _polePosition = FindObjectOfType<PolePositionManager>();
        if (!m_Rigidbody) m_Rigidbody = GetComponent<Rigidbody>();
        if (!_playerInfo) _playerInfo = GetComponent<PlayerInfo>();
    }

    private void Start()
    {
        //this.OnGoingBackwardsEvent += OnGoingBackwardsEventHandler;
        this.OnHasCrashedEvent += OnHasCrashedEventHandler;
        _input.Player.Restart.performed += ctx => CmdReset();
    }

    /// <summary>
    /// Se comprueban los inputs del cliente
    /// </summary>
    public void Update()
    {
        if (isClient)
        {
            InputAcceleration = _input.Player.Acceleration.ReadValue<float>();
            InputSteering = _input.Player.Steering.ReadValue<float>();
            InputBrake = _input.Player.Jump.ReadValue<float>();
            //Speed = m_Rigidbody.velocity.magnitude;

            float r = Mathf.Abs(transform.localRotation.eulerAngles.z);
            Crashed = r > 90 && r < 270;

            //Replace with UI
            if (BackwardsTimeout > 0f )
            {
                goingBackwards = true;
                GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                BackwardsTimeout -= Time.deltaTime;
                OnGoingBackwardsEventHandler(true);
            }
            else
            {
                goingBackwards = false;
                GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
                OnGoingBackwardsEventHandler(false);
            }
        }
    }

    
    public void FixedUpdate()
    {
        //Si el objeto el cliente propio del jugador local se llama a la funcion qeu calcula el movimiento
        if (isLocalPlayer)
        {
            controlMovement(InputSteering, InputAcceleration, InputBrake);
        }
    }

    #endregion



    #region Movimiento
    /// <summary>
    /// Desde el Cliente se llama al servidor para qeu ejecute lso calculos de movimiento.
    /// </summary>
    /// <param name="InputSteering">Entrada de aceleracion</param>
    /// <param name="InputAcceleration">Entrada de giro</param>
    /// <param name="InputBrake">Entrada de freno</param>
    [Command]
    void controlMovement(float InputSteering, float InputAcceleration, float InputBrake)
    {
        //Si no se esta correindo la carrera velocidad = 0
        if (!_polePosition.racing) {
            m_Rigidbody.velocity = Vector3.zero;
            return; }

        InputSteering = Mathf.Clamp(InputSteering, -1, 1);
        InputAcceleration = Mathf.Clamp(InputAcceleration, -1, 1)*2;
        InputBrake = Mathf.Clamp(InputBrake, 0, 1);

        float steering = maxSteeringAngle * InputSteering;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }

            if (axleInfo.motor)
            {

                if (InputAcceleration > float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = forwardMotorTorque;
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.motorTorque = forwardMotorTorque;
                    axleInfo.rightWheel.brakeTorque = 0;
                }

                if (InputAcceleration < -float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = -backwardMotorTorque;
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.motorTorque = -backwardMotorTorque;
                    axleInfo.rightWheel.brakeTorque = 0;
                }

                if (Math.Abs(InputAcceleration) < float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = 0;
                    axleInfo.leftWheel.brakeTorque = engineBrake;
                    axleInfo.rightWheel.motorTorque = 0;
                    axleInfo.rightWheel.brakeTorque = engineBrake;
                }

                if (InputBrake > 0)
                {
                    axleInfo.leftWheel.brakeTorque = footBrake;
                    axleInfo.rightWheel.brakeTorque = footBrake;
                }
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }

        SteerHelper();
        SpeedLimiter();
        AddDownForce();
        TractionControl();

        //Calculo final de la velocidad
        Speed = m_Rigidbody.velocity.magnitude;
    }

    /// <summary>
    /// Tras calcular el movimiento, el servidor avisa a todos los clientes para que muestren por pantalla sus calculos de movimiento
    /// </summary>
    /// <param name="InputSteering"></param>
    /// <param name="InputAcceleration"></param>
    /// <param name="InputBrake"></param>
    [ClientRpc]
    void controlMovementInClient(float InputSteering, float InputAcceleration, float InputBrake)
    {
        float steering = maxSteeringAngle * InputSteering;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit wheelHitLeft;
            WheelHit wheelHitRight;
            axleInfo.leftWheel.GetGroundHit(out wheelHitLeft);
            axleInfo.rightWheel.GetGroundHit(out wheelHitRight);

            if (wheelHitLeft.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitLeft.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.leftWheel.motorTorque -= axleInfo.leftWheel.motorTorque * howMuchSlip * slipLimit;
            }

            if (wheelHitRight.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitRight.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.rightWheel.motorTorque -= axleInfo.rightWheel.motorTorque * howMuchSlip * slipLimit;
            }
        }
    }


    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        foreach (var axleInfo in axleInfos)
        {
            axleInfo.leftWheel.attachedRigidbody.AddForce(
                -transform.up * (downForce * axleInfo.leftWheel.attachedRigidbody.velocity.magnitude));
        }
    }

    private void SpeedLimiter()
    {
        float speed = m_Rigidbody.velocity.magnitude;
        if (speed > topSpeed)
            m_Rigidbody.velocity = topSpeed * m_Rigidbody.velocity.normalized;
    }

    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider col)
    {
        if (col.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = col.transform.GetChild(0);
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);
        var myTransform = visualWheel.transform;
        myTransform.position = position;
        myTransform.rotation = rotation;
    }

    private void SteerHelper()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit[] wheelHit = new WheelHit[2];
            axleInfo.leftWheel.GetGroundHit(out wheelHit[0]);
            axleInfo.rightWheel.GetGroundHit(out wheelHit[1]);
            foreach (var wh in wheelHit)
            {
                if (wh.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(CurrentRotation - transform.eulerAngles.y) < 10f)
        {
            var turnAdjust = (transform.eulerAngles.y - CurrentRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
            m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
        }

        CurrentRotation = transform.eulerAngles.y;
    }
    #endregion


    //NUEVO

    //enables o not the controller when object attached to it is activated or desactivated
    private void OnEnable()
    {
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    //get input Controller
    public _InputController GetInput()
    {
        return _input;
    }

    /// <summary>
    /// EL cliente llama al servidor para qeu calcule la posicion de reseteo al chocar
    /// </summary>
    [Command]
    public void CmdReset()
    {
        //Debug.Log("Digo al server que resetee");
        Reset();
    }

    //if we collide and we are not able to continue playing the car flips
    private void Reset()
    {
        //Debug.Log("Puesto al sitio"); //aqui deberia rotar el coche
        CircuitController cC = FindObjectOfType<CircuitController>();
        int i = GetComponent<PlayerInfo>().LastCheckPoint;

        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        transform.position = cC.checkpoints[i].transform.position;
        transform.rotation = cC.checkpoints[i].transform.rotation;
    }


    /// <summary>
    /// Resetea la posicion y rotacion del PlayerController a las iniciales
    /// </summary>
    /// <param name="t"></param>
    public void ResetToStart(Transform t)
    {
        m_Rigidbody.velocity = Vector3.zero;
        transform.position = t.position;
        transform.rotation = t.rotation;
    }


    /// <summary>
    /// Al chocar con un objeto se calcula, solo si es servidor, la posicion de reseteos
    /// </summary>
    /// <param name="collision">Objeto con el que se choca</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (isServer && collision.relativeVelocity.magnitude > 10f)
        {
            //Debug.Log("Server: " + collision.gameObject.name);
            Reset();
        }
    }

    ///Eventos en caso de choque
    void OnHasCrashedEventHandler(bool hasCrashed)
    {
        _uiManager.ShowCrashedWarning(hasCrashed);
    }

    //Evento en caso de ir marcha atras
    void OnGoingBackwardsEventHandler(bool goingBackwards)
    {
        _uiManager.ShowBackwardsWarning(goingBackwards);
    }

    /// <summary>
    /// hook que se llama cada vez que se modifica el valor de la variable m_CurrentSpeed y se muestra en el InGameHUD
    /// </summary>
    /// <param name="old">Antigua velocidad</param>
    /// <param name="speed">Nueva Velocidad</param>
    [Client]
    public void UpdateSpeed(float old, float speed)
    {
        if (isLocalPlayer)
        {
            _uiManager.UpdateSpeed(speed);
        }
    }
}
