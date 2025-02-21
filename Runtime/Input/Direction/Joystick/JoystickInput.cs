using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class JoystickInput : DisposableBase
    {
        private readonly IJoystick _joystick;
        private readonly MonoDataProvider<Vector3> _cameraEulerAnglesProvider;

        private readonly ReactiveProperty<Vector3?> _worldDirection = new(null);
        public ReadOnlyReactiveProperty<Vector3?> WorldDirection => _worldDirection;


        private readonly Subject<Unit> _onInputBraked = new();
        public Observable<Unit> OnInputBraked => _onInputBraked;



        public JoystickInput(IJoystick joystick, MonoDataProvider<Vector3> cameraEulerAnglesProvider)
        {
            _joystick = joystick ?? throw new ArgumentNullException(nameof(joystick));
            _cameraEulerAnglesProvider = cameraEulerAnglesProvider ?? throw new ArgumentNullException(nameof(cameraEulerAnglesProvider));

            var joystickStream = _joystick.Direction
                .Subscribe(d =>
                {
                    if (d.HasValue)
                    {
                        var screenDirection = ScreenToWorldDirection(d.Value);
                        _worldDirection.Value = GetCameraRelativeDirection(screenDirection);
                    }
                    else _worldDirection.Value = null;
                });


            BuildPermanentDisposable(
                joystickStream, _joystick, _cameraEulerAnglesProvider, _worldDirection, _onInputBraked
            );
        }



        public void BrakeInput()
        {
            ThrowIfDisposed();

            _joystick.BrakeInput();
            _onInputBraked.OnNext(Unit.Default);
            _worldDirection.Value = null;
        }



        public static Vector3 ScreenToWorldDirection(Vector2 screenDirection)
        {
            return new(screenDirection.x, 0, screenDirection.y);
        }

        public Vector3 GetCameraRelativeDirection(Vector3 direction)
        {
            ThrowIfDisposed();

            var cameraRotation = Quaternion.Euler(
                0,
                _cameraEulerAnglesProvider.Value.y,
                _cameraEulerAnglesProvider.Value.z
            );

            return cameraRotation * direction;
        }
    }
}