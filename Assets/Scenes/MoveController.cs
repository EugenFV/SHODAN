////#define FORCE_USE_MOBILE_CONTROL

//using GameUI;
//using GameUI.Battle;
//using Tutorial.Battle;
//using UnityEngine;
//using UnityGui.GameUnityUI;

//public class MoveController : MonoBehaviour
//{
//    public float MotorPredictionSmooth = 0.25f;
//    public long MotorPrediction = 10;
//    public Vector3 InputDirection;
//    #region Move Control
//    protected GameObject calculator = null;
//    protected float horizontalAxis = 0;
//    protected float verticalAxis = 0;
//    bool customAxisControl = true;
//    public float customAxisAcceleration = 0.05f;
//    Vector3 avePosition = Vector3.zero;
//    protected bool inAir;
//    public bool InAir
//    {
//        get { return inAir; }
//        private set
//        {
//            if (inAir != value)
//            {
//                inAir = value;
//                //UpdateGravity(true);
//            }
//        }
//    }
//    public bool InAirLocal
//    {
//        get;
//        private set;
//    }

//    public bool UseMotorPrediction
//    {
//        get
//        {
//            return MotorPrediction >= 0;
//        }
//    }

//    public const long MOVE_OFF_DELAY = 1000000; //100ms
//    public const int MOVE_CONTROL_RADIUS = 64;
//    protected float gravityMultiplier = 1f;
//    protected float lastVerticalVelocity;
//    public const float MIN_MOVE_SPEED = 30f;

//    public TimeSpaceCurve timeSpaceCurve = new TimeSpaceCurve();

//    private float? runningSetStartTime = null;

//    public bool IsRunning { get; private set; } = false;
//    public bool IsMoving { get; private set; } = false;

//    public bool IsSprinting { get; set; }
//    protected CharacterMotor _motor;
//    public CharacterMotor Motor
//    {
//        get
//        {
//            return _motor;
//        }
//    }

//    private bool _isSprintingAvaliable = false;
//    public bool IsSprintingAvaliable
//    {
//        get
//        {
//            return _isSprintingAvaliable;
//        }
//    }
//    public string ColliderTag
//    {
//        get { return Motor.ColliderTag; }
//    }

//    public bool IsBlocked { get; private set; }
//    #endregion Move Control

//    protected ISoldierController _soldierController;

//    public void Block(bool block)
//    {
//        IsBlocked = block;
//        if (block)
//        {
//            InputDirection = Vector3.zero;
//            _soldierController.LocalPlayer.MotionState.ResetState();
//            UpdateRunning(Vector3.zero, _soldierController.LocalPlayer.MotionState);
//            UpdateMotor(Vector3.zero, _soldierController.LocalPlayer.MotionState, 0);
//            UpdateGUIController(Vector3.zero, IsRunning && _soldierController.SoldierControlSettings.MotorSettings.RunSpeed != _soldierController.SoldierControlSettings.MotorSettings.Speed);
//            UpdatePosition();
//        }
//        if (enabled != !block)
//        {
//            enabled = !block;
//        }
//    }

//    public virtual void Init(ISoldierController soldierController)
//    {
//        _soldierController = soldierController;
//        InitPrediction(soldierController.SoldierControlSettings);
//        SetupMotor(_soldierController.SoldierControlSettings.MotorSettings);
//        runningSetStartTime = null;
//        _isSprintingAvaliable = soldierController.SoldierControlSettings.MotorSettings.RunSpeed != soldierController.SoldierControlSettings.MotorSettings.SprintSpeed;
//    }

//    protected void InitPrediction(SoldierControlSettings soldierControlSettings)
//    {
//        SetMotor();
//        if (UseMotorPrediction)
//        {
//            MotorPredictionSmooth = Configuration.MotorPredictionSmooth;
//            MotorPrediction = Configuration.MotorPrediction;
//            SetupPredictionCalculator(soldierControlSettings);
//            ResetMotor();
//        }
//    }
//    protected void SetMotor()
//    {
//        _motor = _soldierController.LocalPlayer.transform.GetComponent<CharacterMotor>();
//    }
//    protected void ResetMotor()
//    {
//        CharacterController controller = gameObject.GetComponent<CharacterController>();
//        CharacterPusher pusher = gameObject.GetComponent<CharacterPusher>();
//        controller.enabled = false;
//        _motor.enabled = false;
//        pusher.enabled = false;
//        _motor = calculator.GetComponent<CharacterMotor>();
//    }
//    protected void SetupPredictionCalculator(SoldierControlSettings soldierControlSettings)
//    {
//        calculator = CharacterManager.GetPlayerCalculator();
//        calculator.transform.SetParent(this.transform.parent);
//        calculator.transform.position = this.transform.position;
//        calculator.transform.localRotation = this.transform.localRotation;
//    }
//    protected void SetupMotor(MotorSettings motorSettings)
//    {
//        if (UseMotorPrediction)
//        {
//            Motor.movementSettings.maxGroundAcceleration = float.MaxValue;
//            Motor.movementSettings.maxGroundDecceration = float.MaxValue;
//            Motor.movementSettings.maxAirAcceleration = float.MaxValue;
//            Motor.movementSettings.maxAirDecceration = float.MaxValue;
//        }
//        else
//        {
//            Motor.movementSettings.maxGroundAcceleration = motorSettings.GroundAcceleration;
//            Motor.movementSettings.maxGroundDecceration = motorSettings.GroundDecceration;
//            Motor.movementSettings.maxAirAcceleration = motorSettings.AirAcceleration;
//            Motor.movementSettings.maxAirDecceration = motorSettings.AirDecceration;
//        }
//        Motor.movementSettings.jumpBaseHeight = motorSettings.JumpHeight;
//    }

//    public void UpdateFunction(bool isControllBlocked, PlayerMotionState motionState, float weaponSpeedMultiplier)
//    {
//        if (!isActiveAndEnabled || IsBlocked)
//        {
//            return;
//        }
//        Vector3 moveDir = UpdateMoveDirection(isControllBlocked);
//        Vector3 normalDir = NormalizeMoveDirection(moveDir);
//        InputDirection = normalDir;
//        UpdateRunning(moveDir, motionState);
//        UpdateMotor(normalDir, motionState, weaponSpeedMultiplier);
//        UpdateGUIController(normalDir, IsRunning && _soldierController.SoldierControlSettings.MotorSettings.RunSpeed != _soldierController.SoldierControlSettings.MotorSettings.Speed);
//        UpdatePosition();
//    }

//    protected virtual Vector3 UpdateMoveDirection(bool isControllBlocked)
//    {
//        Vector3 moveDir = Vector3.zero;
//        //if (!this.soldierCameraController.SoldierCameraExtension.BlockMovement)
//        {
//            if (isControllBlocked)
//            {
//                //moveDir = Vector3.zero;
//                Motor.CanControl = false;
//                return moveDir;
//            }
//            else
//            {
//                Motor.CanControl = true;
//            }
//        }
//#if (UNITY_EDITOR || UNITY_STANDALONE) && !FORCE_USE_MOBILE_CONTROL
//        moveDir = UpdateMoveDirectionEditor();
//#else
//        moveDir = UpdateMoveDirectionMobile();
//#endif      
//        float moveDirSqrMagnitude = moveDir.sqrMagnitude;
//        if (moveDirSqrMagnitude > 0.0001f)
//        {
//            if (moveDirSqrMagnitude < MIN_MOVE_SPEED * MIN_MOVE_SPEED)
//            {
//                moveDir = moveDir.normalized * MIN_MOVE_SPEED;
//            }
//        }
//        else
//        {
//            moveDir = Vector3.zero;
//        }
//        return moveDir;
//    }

//    protected void UpdateMotor(Vector3 normalDir, PlayerMotionState motionState, float weaponSpeedMultiplier)
//    {
//        MotorSettings motorSettings = _soldierController.SoldierControlSettings.MotorSettings;
//        //Update Motor Control
//        Motor.InputMoveDirection = normalDir;
//        Motor.transform.rotation = transform.rotation;
//        Motor.InputJumpPressed = IsJumpInputActive() && (!(motionState.InAirLocal || ColliderTag == "") || Motor.IsClimb);
//        //Update Motor Speed
//        float speed = UpdateSpeed(weaponSpeedMultiplier, motionState);
//        Motor.movementSettings.maxForwardSpeed = speed;
//        Motor.movementSettings.maxBackwardsSpeed = speed;
//        Motor.movementSettings.maxSidewaysSpeed = speed * motorSettings.StrafeCoefficient;
//        Motor.movementSettings.jumpBaseHeight = motorSettings.Jump;
//        //Update Motor
//        Motor.UpdateFunction();
//    }

//    protected void UpdatePosition()
//    {
//        if (UseMotorPrediction)
//        {
//            UpdatePredictedPosition();
//        }
//        else
//        {
//            avePosition = Motor.transform.position;
//            InAirLocal = InAir = Motor.IsGrounded;
//        }
//        _soldierController.Position = avePosition;
//    }
//    protected void UpdatePredictedPosition()
//    {
//        Vector3 position = avePosition;
//        timeSpaceCurve.Push(calculator.transform.position, calculator.transform.localEulerAngles, Motor.IsGrounded, BattleServerTimeManager.Instance.NetworkTime);
//        TimeSpaceTransform transform = timeSpaceCurve.Pop(BattleServerTimeManager.Instance.NetworkTime - MotorPrediction);
//        position = transform.Position;

//        InAir = !Motor.IsGrounded;  
//        InAirLocal = !transform.Grounded;
//        avePosition = Vector3.Lerp(avePosition, position, MotorPredictionSmooth);
//    }

//    public Vector3 UpdateMoveDirectionEditor()
//    {
//        Vector3 moveDir;
//        if (customAxisControl)
//        {
//            GetEditorInput();
//            moveDir = new Vector3(horizontalAxis, 0, verticalAxis) * 10000;
//        }
//        else
//        {
//            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * 10000;
//        }
//        return moveDir;
//    }
//    public Vector3 UpdateMoveDirectionMobile()
//    {
//        Vector3 moveDir = Vector3.zero;
//        if (false && Input.GetJoystickNames().Length > 0)
//        {
//            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
//        }
//        else
//        {
//#if (UNITY_EDITOR || UNITY_STANDALONE)
//            moveDir = GetTouchDirectionFixedMobileEditorEmulation();
//#else
//            moveDir = GetTouchDirectionFixedMobile();
//#endif
//        }
//        return moveDir;
//    }

//    private Vector3 GetTouchDirectionFixedMobile()
//    {
//        Vector3 moveDir = Vector3.zero;
//        bool hasMoveTouch = false;
//        foreach (TouchInstance touch in _soldierController.InputManager.Touches.Values)
//        {
//            if (touch.StartPosition.x < Screen.width / 2 && touch.StartPosition.y < Screen.height / 2)
//            {
//                switch (touch.Touch.phase)
//                {
//                    case TouchPhase.Moved:
//                    case TouchPhase.Stationary:
//                        Vector2 joysticScreenPosition = BattleUIManager.Instance.HUD.GetMovementJoystickScreenPos();//qwerty
//                        moveDir = new Vector3(joysticScreenPosition.x, 0, joysticScreenPosition.y);
//                        hasMoveTouch = true;
//                        break;
//                }
//                break;
//            }
//        }
//        if (!hasMoveTouch)
//        {
//            moveDir = Vector3.zero;
//        }
//        return moveDir;
//    }

//    private Vector3 GetTouchDirectionFixedMobileEditorEmulation()
//    {
//        Vector3 moveDir = Vector3.zero;
//        bool hasMoveTouch = false;
//        if (Input.GetMouseButton(0))
//        {
//            Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
//            if (mousePosition.x < Screen.width / 2 && mousePosition.y < Screen.height / 2)
//            {
//                Vector2 joysticScreenPosition = BattleUIManager.Instance.HUD.GetMovementJoystickScreenPos();
//                moveDir = new Vector3(joysticScreenPosition.x, 0, joysticScreenPosition.y);
//                hasMoveTouch = true;
//            }
//        }
//        if (!hasMoveTouch)
//        {
//            moveDir = Vector3.zero;
//        }
//        return moveDir;
//    }
//    public void GetEditorInput()
//    {
//        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(TRInput.LeftStrafe))
//        {
//            if (horizontalAxis > 0)
//            {
//                horizontalAxis = 0;
//            }
//            if (horizontalAxis > -1 + customAxisAcceleration / 2)
//            {
//                horizontalAxis -= customAxisAcceleration;
//            }
//        }
//        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(TRInput.RightStrafe))
//        {
//            if (horizontalAxis < 0)
//            {
//                horizontalAxis = 0;
//            }
//            if (horizontalAxis < 1 - customAxisAcceleration / 2)
//            {
//                horizontalAxis += customAxisAcceleration;
//            }
//        }
//        else
//        {
//            if (horizontalAxis > customAxisAcceleration / 2)
//            {
//                horizontalAxis -= customAxisAcceleration;
//            }
//            else if (horizontalAxis < -customAxisAcceleration / 2)
//            {
//                horizontalAxis += customAxisAcceleration;
//            }
//        }

//        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(TRInput.Forward))
//        {
//            if (verticalAxis < 0)
//            {
//                verticalAxis = 0;
//            }
//            if (verticalAxis < 1 - customAxisAcceleration / 2)
//            {
//                verticalAxis += customAxisAcceleration;
//            }
//        }
//        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(TRInput.Backward))
//        {
//            if (verticalAxis > 0)
//            {
//                verticalAxis = 0;
//            }
//            if (verticalAxis > -1 + customAxisAcceleration / 2)
//            {
//                verticalAxis -= customAxisAcceleration;
//            }
//        }
//        else
//        {
//            if (verticalAxis > customAxisAcceleration / 2)
//            {
//                verticalAxis -= customAxisAcceleration;
//            }
//            else if (verticalAxis < -customAxisAcceleration / 2)
//            {
//                verticalAxis += customAxisAcceleration;
//            }
//        }
//    }
//    protected virtual bool IsJumpInputActive()
//    {
//#if (UNITY_EDITOR || UNITY_STANDALONE) && !FORCE_USE_MOBILE_CONTROL
//        return Input.GetKey(TRInput.Jump);// && !player.MotionState.Crouch;
//#else
//        if (BattleUIManager.Instance.HUD.IsJumpBtnPressed && _soldierController.LocalPlayer.MotionState.Crouch)
//        {
//            BattleUIManager.Instance.HUD.IsCrouchBtnPressed = false;
//        }
//        return BattleUIManager.Instance.HUD.IsJumpBtnPressed;
//#endif
//    }

//    protected virtual float UpdateSpeed(float weaponSpeedMultiplier, PlayerMotionState motionState)
//    {
//        float speed = _soldierController.SoldierControlSettings.GetSpeed(!IsRunning && !motionState.Aim, motionState.Walk || motionState.Crouch || motionState.Stunned, IsSprinting);
//        speed = speed * weaponSpeedMultiplier;
//        return speed;
//    }
//    protected Vector3 NormalizeMoveDirection(Vector3 moveDir)
//    {
//        Vector3 normalDir = Vector3.zero;
//        float walkDirDelta = BattleUIManager.Instance.HUD.GetMovementJoystickScreenRadius();//qwerty
//        float moveDirSqrMagnitude = moveDir.sqrMagnitude;
//        if (moveDirSqrMagnitude < walkDirDelta)
//        {
//            normalDir = moveDir / Mathf.Sqrt(walkDirDelta);
//        }
//        else if (moveDirSqrMagnitude > MOVE_CONTROL_RADIUS)
//        {
//            normalDir = moveDir.normalized;
//        }
//        return normalDir;
//    }
//    protected void UpdateGUIController(Vector3 normalDir, bool isRunning)
//    {
//        if (isRunning)
//        {
//            BattleUIManager.Instance.HUD.SetMovementJoystickInsideMarkerPos(new Vector2(normalDir.x, normalDir.z * 1.5f));//qwerty
//        }
//        else
//        {
//            BattleUIManager.Instance.HUD.SetMovementJoystickInsideMarkerPos(new Vector2(normalDir.x, normalDir.z));//qwerty
//        }

//        if (IsSprintingAvaliable)
//        {
//            BattleUIManager.Instance.HUD.SetSprintBtnGOState(isRunning);
//        }
//    }
//    protected bool UpdateRunning(Vector3 moveDir, PlayerMotionState motionState)
//    {
//        float walkDirDelta = BattleUIManager.Instance.HUD.GetMovementJoystickScreenRadius();//qwerty
//        float moveDirectionSqr = (moveDir.z * moveDir.z) * Mathf.Sign(moveDir.z);
//        SetIsRunning(moveDirectionSqr > walkDirDelta && Mathf.Abs(moveDir.z) >= Mathf.Abs(moveDir.x) && !motionState.Aim && !motionState.Shooting && !motionState.InAirLocal && !motionState.Sprinting);
//        SetIsMoving(moveDir != Vector3.zero);
//        return IsRunning;
//    }
//    public void SetIsRunning(bool on)
//    {
//        if (on)
//        {
//            float time = Time.time;
//            if (runningSetStartTime == null)
//            {
//                runningSetStartTime = Time.time;
//            }
//            if (time - runningSetStartTime > Configuration.RunOnDelay)
//            {
//                IsRunning = true;
//            }
//        }
//        else
//        {
//            runningSetStartTime = null;
//            IsRunning = false;
//        }
//    }
//    public void SetIsMoving(bool on)
//    {
//        IsMoving = on;
//    }
//    public void Spawn(Vector3 position)
//    {
//        if (calculator != null)
//        {
//            calculator.transform.position = this.transform.position;
//            calculator.transform.localRotation = this.transform.localRotation;
//            avePosition = calculator.transform.position = position;
//        }
//        else
//        {
//            avePosition = position;
//        }

//        InAir = InAirLocal = false;
//        IsSprinting = false;
//        verticalAxis = horizontalAxis = 0;

//        Motor.Reset();
//    }
//    public void Kill()
//    {
//        Motor.Reset();
//    }
//    public void SetOrientation(Vector3 rotation)
//    {
//        if (calculator != null)
//        {
//            calculator.transform.localEulerAngles = new Vector3(0, rotation.y, 0);
//        }
//    }
//    public Vector3 GetPosition()
//    {
//        if (calculator != null)
//        {
//            return calculator.transform.position;
//        }
//        return this.transform.position;
//    }

//}
