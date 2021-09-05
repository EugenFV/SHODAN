using Encog.Neural.Networks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FPSControllerLPFP
{
    /// Manages a first person character
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(AudioSource))]
    public class FpsControllerLPFP : MonoBehaviour
    {
#pragma warning disable 649
		[Header("Arms")]
        [Tooltip("The transform component that holds the gun camera."), SerializeField]
        private Transform arms;

        [Tooltip("The position of the arms and gun camera relative to the fps controller GameObject."), SerializeField]
        private Vector3 armPosition;

		[Header("Audio Clips")]
        [Tooltip("The audio clip that is played while walking."), SerializeField]
        private AudioClip walkingSound;

        [Tooltip("The audio clip that is played while running."), SerializeField]
        private AudioClip runningSound;

		[Header("Movement Settings")]
        [Tooltip("How fast the player moves while walking and strafing."), SerializeField]
        private float walkingSpeed = 5f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float runningSpeed = 9f;

        [Tooltip("Approximately the amount of time it will take for the player to reach maximum running or walking speed."), SerializeField]
        private float movementSmoothness = 0.125f;

        [Tooltip("Amount of force applied to the player when jumping."), SerializeField]
        private float jumpForce = 35f;

		[Header("Look Settings")]
        [Tooltip("Rotation speed of the fps controller."), SerializeField]
        private float mouseSensitivity = 7f;

        [Tooltip("Approximately the amount of time it will take for the fps controller to reach maximum rotation speed."), SerializeField]
        private float rotationSmoothness = 0.05f;

        [Tooltip("Minimum rotation of the arms and camera on the x axis."),
         SerializeField]
        private float minVerticalAngle = -90f;

        [Tooltip("Maximum rotation of the arms and camera on the axis."),
         SerializeField]
        private float maxVerticalAngle = 90f;

        [Tooltip("The names of the axes and buttons for Unity's Input Manager."), SerializeField]
        private FpsInput input;
#pragma warning restore 649

        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private AudioSource _audioSource;
        private SmoothRotation _rotationX;
        private SmoothRotation _rotationY;
        private SmoothVelocity _velocityX;
        private SmoothVelocity _velocityZ;
        private bool _isGrounded;

        private readonly RaycastHit[] _groundCastResults = new RaycastHit[8];
        private readonly RaycastHit[] _wallCastResults = new RaycastHit[8];

        [SerializeField] private Sensor[] sensors;
        BasicNetwork network = null;

        /// Initializes the FpsController on start.
        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _collider = GetComponent<CapsuleCollider>();
            _audioSource = GetComponent<AudioSource>();
			arms = AssignCharactersCamera();
            _audioSource.clip = walkingSound;
            _audioSource.loop = true;
            _rotationX = new SmoothRotation(RotationXRaw);
            _rotationY = new SmoothRotation(RotationYRaw);
            _velocityX = new SmoothVelocity();
            _velocityZ = new SmoothVelocity();
            Cursor.lockState = CursorLockMode.Locked;
            ValidateRotationRestriction();

            network = (BasicNetwork)Encog.Util.SerializeObject.Load("shodan.ser");
        }
			
        private Transform AssignCharactersCamera()
        {
            var t = transform;
			arms.SetPositionAndRotation(t.position, t.rotation);
			return arms;
        }
        
        /// Clamps <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> to valid values and
        /// ensures that <see cref="minVerticalAngle"/> is less than <see cref="maxVerticalAngle"/>.
        private void ValidateRotationRestriction()
        {
            minVerticalAngle = ClampRotationRestriction(minVerticalAngle, -90, 90);
            maxVerticalAngle = ClampRotationRestriction(maxVerticalAngle, -90, 90);
            if (maxVerticalAngle >= minVerticalAngle) return;
            Debug.LogWarning("maxVerticalAngle should be greater than minVerticalAngle.");
            var min = minVerticalAngle;
            minVerticalAngle = maxVerticalAngle;
            maxVerticalAngle = min;
        }

        private static float ClampRotationRestriction(float rotationRestriction, float min, float max)
        {
            if (rotationRestriction >= min && rotationRestriction <= max) return rotationRestriction;
            var message = string.Format("Rotation restrictions should be between {0} and {1} degrees.", min, max);
            Debug.LogWarning(message);
            return Mathf.Clamp(rotationRestriction, min, max);
        }
			
        /// Checks if the character is on the ground.
        private void OnCollisionStay()
        {
            var bounds = _collider.bounds;
            var extents = bounds.extents;
            var radius = extents.x - 0.01f;
            Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
                _groundCastResults, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
            if (!_groundCastResults.Any(hit => hit.collider != null && hit.collider != _collider)) return;
            for (var i = 0; i < _groundCastResults.Length; i++)
            {
                _groundCastResults[i] = new RaycastHit();
            }

            _isGrounded = true;
        }

        private bool isSave = false;
        private bool isRun = false;
        private bool isEmpty = true;
        private bool isRetrain = false;

        MyPatterns myObjects = new MyPatterns();
        MyPatterns AddObjects = new MyPatterns();

        /// Processes the character movement and the camera rotation every fixed framerate frame.
        private void FixedUpdate() //QWERTY
        {
            GetSensors();
            // FixedUpdate is used instead of Update because this code is dealing with physics and smoothing.
            RotateCameraAndCharacter();
            MoveCharacter();
            _isGrounded = false;
        }

        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        private void Update()
        {
            arms.position = transform.position + transform.TransformVector(armPosition);
            Jump();
            PlayFootstepSounds();
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            //return;
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.L)
                {
                    if (isSave)
                    {
                        isSave = false;
                    }
                    else
                    {
                        isSave = true;
                        Debug.LogError($"Start Recording");
                    }
                }

                if (e.keyCode == KeyCode.O)
                {
                    if (isRun)
                    {
                        isRun = false;
                    }
                    else
                    {
                        isRun = true;
                    }
                }

                if (e.keyCode == KeyCode.P)
                {
                    if (isRetrain)
                    {
                        isRetrain = false;
                    }
                    else
                    {
                        isRetrain = true;
                        AddObjects.Patterns.Clear();
                    }
                }
            }
        }
#endif

        
        private void GetSensors()
        {
            
            //Debug.LogError("x" + NormalizeAngle(arms.eulerAngles.x));
            //Debug.LogError("z" + arms.eulerAngles.z);
            //_rotationY.Current
            if (isRun)
            {
                RunShodan();
                return;
            }

            if (isRetrain)
            {
                RetrainShodan();
                return;
            }

            if (!isSave)
                return;

            if (myObjects.Patterns.Count % 100 == 0)
            {
                Debug.LogError($"myObjects.Patterns.Count: {myObjects.Patterns.Count}");
            }
            //Debug.LogError($"GetSensors()");
            
            if (myObjects.Patterns.Count > 4000)
            { 
                isSave = false;
                string json = JsonUtility.ToJson(myObjects);
                string PATH_SHODAN = Application.dataPath + "/Shodan.txt";
                File.WriteAllText(PATH_SHODAN, json);
                Debug.LogError($"Shodan Saved: {PATH_SHODAN}");
            }

            double[] sensorOutput = new double[sensors.Length + 1];
            sensorOutput[0] = AnglesToOne(NormalizeAngle(arms.eulerAngles.x));

            for (var i = 1; i < sensors.Length; ++i)
            {
                sensorOutput[i] = sensors[i].Output * 0.1f;//  TenNormalize(sensors[i].Output);
                //Debug.LogError(sensorOutput[i]);
            }
            Pattern patern = new Pattern();

            patern.Inputs = sensorOutput;

            var rotationXRaw = TenNormalize(RotationXRaw);
            var rotationYRaw = TenNormalize(RotationYRaw);
            var moveRaw = OneNormalize(MoveRaw);
            var strafeRaw = OneNormalize(StrafeRaw);
             
            patern.Outputs = new double[]{rotationXRaw, rotationYRaw, moveRaw, strafeRaw};

            myObjects.Patterns.Add(patern);
        }

        private void RetrainShodan()
        {
            if (AddObjects.Patterns.Count > 200)
            {
                isRetrain = false;
                string PATH_SHODAN = Application.dataPath + "/Shodan.txt";
                string data = File.ReadAllText(PATH_SHODAN);
                myObjects = JsonUtility.FromJson<MyPatterns>(data);

                myObjects.Patterns.AddRange(AddObjects.Patterns);

                string json = JsonUtility.ToJson(myObjects);
                File.WriteAllText(PATH_SHODAN, json);
                Debug.LogError($"Shodan Saved: {PATH_SHODAN}");
            }

            double[] sensorOutput = new double[sensors.Length + 1];
            sensorOutput[0] = AnglesToOne(NormalizeAngle(arms.eulerAngles.x));

            for (var i = 1; i < sensors.Length; ++i)
            {
                sensorOutput[i] = sensors[i].Output * 0.1f;//  TenNormalize(sensors[i].Output);
                //Debug.LogError(sensorOutput[i]);
            }
            Pattern patern = new Pattern();

            patern.Inputs = sensorOutput;

            var rotationXRaw = TenNormalize(RotationXRaw);
            var rotationYRaw = TenNormalize(RotationYRaw);
            var moveRaw = OneNormalize(MoveRaw);
            var strafeRaw = OneNormalize(StrafeRaw);

            patern.Outputs = new double[] { rotationXRaw, rotationYRaw, moveRaw, strafeRaw };

            AddObjects.Patterns.Add(patern);
        }

        private void RunShodan()
        {
            double[] sensorResult = new double[4];
            double[] sensorInputs = new double[sensors.Length+1];

            sensorInputs[0] = OneToOnes(arms.rotation.x);
            for (var i = 0; i < sensors.Length; ++i)
            {
                sensorInputs[i] = sensors[i].Output * 0.1f;
            }
            network.Compute(sensorInputs, sensorResult);

            ShodanRotationXRaw = OneToTens((float)sensorResult[0]);
            ShodanRotationYRaw = OneToTens((float)sensorResult[1]);
            ShodanMoveRaw = OneToOnes((float)sensorResult[2]);
            ShodanStrafeRaw = OneToOnes((float)sensorResult[3]);
        }

        private void RotateCameraAndCharacter()
        {
            var rotationX = _rotationX.Update(RotationXRaw, rotationSmoothness);
            var rotationY = _rotationY.Update(RotationYRaw, rotationSmoothness);
            var clampedY = RestrictVerticalRotation(rotationY);
            _rotationY.Current = clampedY;
			var worldUp = arms.InverseTransformDirection(Vector3.up);
			var rotation = arms.rotation *
                           Quaternion.AngleAxis(rotationX, worldUp) *
                           Quaternion.AngleAxis(clampedY, Vector3.left);
            transform.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
			arms.rotation = rotation;
        }

        float ShodanRotationXRaw = 0.0f;
        /// Returns the target rotation of the camera around the y axis with no smoothing.
        private float RotationXRaw
        {
            get
            {
                if (isRun)
                {
                    return ShodanRotationXRaw;
                }

                //Debug.LogError($"X = {input.RotateX * mouseSensitivity}");
                var inputBuffer = input.RotateX * mouseSensitivity;
                if (inputBuffer > 10)
                {
                    return inputBuffer = 10;
                }
                else if (inputBuffer < -10)
                {
                    return inputBuffer = -10;
                }
                return inputBuffer;
            }
        }

        float ShodanRotationYRaw = 0.0f;
        /// Returns the target rotation of the camera around the x axis with no smoothing.
        private float RotationYRaw
        {
            get 
            {
                
                float delta = 0.0f;

                //if (isRetrain)
                //{
                    //if (NormalizeAngle(arms.eulerAngles.x) > 0)
                    //{
                    //    delta = 0.5f;
                    //}
                    //else if (NormalizeAngle(arms.eulerAngles.x) < 0)
                    //{
                    //    delta = -0.5f;
                    //}
                //}

                if (isRun)
                {
                    return ShodanRotationYRaw;
                }

                var inputBuffer = input.RotateY * mouseSensitivity + delta;
                if (inputBuffer > 10)
                {
                    return inputBuffer = 10;
                }
                else if (inputBuffer < -10)
                {
                    return inputBuffer = -10;
                }
                return inputBuffer;
            }
        }

        float ShodanMoveRaw = 0.0f;
        public float MoveRaw
        {
            //Debug.LogError($"X = {input.RotateX * mouseSensitivity}");
            get
            {
                if (isRun)
                {
                    return ShodanMoveRaw;
                }
                //Debug.LogError($"Move = {input.Move}");
                return input.Move;
            }
        }


        float ShodanStrafeRaw = 0.0f;
        /// Returns the value of the virtual axis mapped to move the character left and right.         
        public float StrafeRaw
        {
            get 
            {
                if (isRun)
                {
                    return ShodanStrafeRaw;
                }
                //Debug.LogError($"Strafe = {input.Strafe}");
                return input.Strafe;
            }
        }

        public float OneToTens(float value)
        {
            return 20.0f * value - 10.0f; 
        }

        public float TenNormalize(float value)
        {
            return (value + 10.0f) * 0.05f;
        }

        public float OneNormalize(float value)
        {
            return (value + 1.0f) * 0.5f;
        }

        public float OneToOnes(float value)
        {
            return 2.0f * value - 1.0f;
        }

        public float AnglesToOne(float value)
        {
            return (value + 90) * 0.00555f;
        }


        /// Clamps the rotation of the camera around the x axis
        /// between the <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> values.
        private float RestrictVerticalRotation(float mouseY)
        {
			var currentAngle = NormalizeAngle(arms.eulerAngles.x);
            var minY = minVerticalAngle + currentAngle;
            var maxY = maxVerticalAngle + currentAngle;
            return Mathf.Clamp(mouseY, minY + 0.01f, maxY - 0.01f);
        }
			
        /// Normalize an angle between -180 and 180 degrees.
        /// <param name="angleDegrees">angle to normalize</param>
        /// <returns>normalized angle</returns>
        private static float NormalizeAngle(float angleDegrees)
        {
            while (angleDegrees > 180f)
            {
                angleDegrees -= 360f;
            }

            while (angleDegrees <= -180f)
            {
                angleDegrees += 360f;
            }

            return angleDegrees;
        }

        private void MoveCharacter()
        {
            var direction = new Vector3(MoveRaw, 0f, StrafeRaw).normalized;
            var worldDirection = transform.TransformDirection(direction);
            var velocity = worldDirection * (input.Run ? runningSpeed : walkingSpeed);
            //Checks for collisions so that the character does not stuck when jumping against walls.
            var intersectsWall = CheckCollisionsWithWalls(velocity);
            if (intersectsWall)
            {
                _velocityX.Current = _velocityZ.Current = 0f;
                return;
            }

            var smoothX = _velocityX.Update(velocity.x, movementSmoothness);
            var smoothZ = _velocityZ.Update(velocity.z, movementSmoothness);
            var rigidbodyVelocity = _rigidbody.velocity;
            var force = new Vector3(smoothX - rigidbodyVelocity.x, 0f, smoothZ - rigidbodyVelocity.z);
            _rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        private bool CheckCollisionsWithWalls(Vector3 velocity)
        {
            if (_isGrounded) return false;
            var bounds = _collider.bounds;
            var radius = _collider.radius;
            var halfHeight = _collider.height * 0.5f - radius * 1.0f;
            var point1 = bounds.center;
            point1.y += halfHeight;
            var point2 = bounds.center;
            point2.y -= halfHeight;
            Physics.CapsuleCastNonAlloc(point1, point2, radius, velocity.normalized, _wallCastResults,
                radius * 0.04f, ~0, QueryTriggerInteraction.Ignore);
            var collides = _wallCastResults.Any(hit => hit.collider != null && hit.collider != _collider);
            if (!collides) return false;
            for (var i = 0; i < _wallCastResults.Length; i++)
            {
                _wallCastResults[i] = new RaycastHit();
            }

            return true;
        }

        private void Jump()
        {
            if (!_isGrounded || !input.Jump) return;
            _isGrounded = false;
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void PlayFootstepSounds()
        {
            if (_isGrounded && _rigidbody.velocity.sqrMagnitude > 0.1f)
            {
                _audioSource.clip = input.Run ? runningSound : walkingSound;
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                }
            }
            else
            {
                if (_audioSource.isPlaying)
                {
                    _audioSource.Pause();
                }
            }
        }
			
        /// A helper for assistance with smoothing the camera rotation.
        private class SmoothRotation
        {
            private float _current;
            private float _currentVelocity;

            public SmoothRotation(float startAngle)
            {
                _current = startAngle;
            }
				
            /// Returns the smoothed rotation.
            public float Update(float target, float smoothTime)
            {
                return _current = Mathf.SmoothDampAngle(_current, target, ref _currentVelocity, smoothTime);
            }

            public float Current
            {
                set { _current = value; }
            }
        }
			
        /// A helper for assistance with smoothing the movement.
        private class SmoothVelocity
        {
            private float _current;
            private float _currentVelocity;

            /// Returns the smoothed velocity.
            public float Update(float target, float smoothTime)
            {
                return _current = Mathf.SmoothDamp(_current, target, ref _currentVelocity, smoothTime);
            }

            public float Current
            {
                set { _current = value; }
            }
        }
			
        /// Input mappings
        [Serializable]
        private class FpsInput
        {
            [Tooltip("The name of the virtual axis mapped to rotate the camera around the y axis."),
             SerializeField]
            private string rotateX = "Mouse X";

            [Tooltip("The name of the virtual axis mapped to rotate the camera around the x axis."),
             SerializeField]
            private string rotateY = "Mouse Y";

            [Tooltip("The name of the virtual axis mapped to move the character back and forth."),
             SerializeField]
            private string move = "Horizontal";

            [Tooltip("The name of the virtual axis mapped to move the character left and right."),
             SerializeField]
            private string strafe = "Vertical";

            [Tooltip("The name of the virtual button mapped to run."),
             SerializeField]
            private string run = "Fire3";

            [Tooltip("The name of the virtual button mapped to jump."),
             SerializeField]
            private string jump = "Jump";

            /// Returns the value of the virtual axis mapped to rotate the camera around the y axis.
            public float RotateX
            {
                get { return Input.GetAxisRaw(rotateX); }
            }
				         
            /// Returns the value of the virtual axis mapped to rotate the camera around the x axis.        
            public float RotateY
            {
                get { return Input.GetAxisRaw(rotateY); }
            }
				        
            /// Returns the value of the virtual axis mapped to move the character back and forth.        
            public float Move
            {
                get { return Input.GetAxisRaw(move); }
            }
				       
            /// Returns the value of the virtual axis mapped to move the character left and right.         
            public float Strafe
            {
                get { return Input.GetAxisRaw(strafe); }
            }
				    
            /// Returns true while the virtual button mapped to run is held down.          
            public bool Run
            {
                get { return Input.GetButton(run); }
            }
				     
            /// Returns true during the frame the user pressed down the virtual button mapped to jump.          
            public bool Jump
            {
                get { return Input.GetButtonDown(jump); }
            }

        }
    }

    [Serializable]
    public class MyPatterns
    {
        public List<Pattern> Patterns = new List<Pattern>();
    }

    [Serializable]
    public class Pattern
    {
        public double[] Inputs;
        public double[] Outputs;
    }
}