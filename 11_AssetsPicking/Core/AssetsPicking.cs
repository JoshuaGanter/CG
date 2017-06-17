using System;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static System.Math;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Tutorial.Core
{
    public class AssetsPicking : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private ScenePicker _scenePicker;
        private float _camAngle = -2.335f;
        private float _camAngleVelocity = 0;
        private PickResult _currentPick;
        private float3 _oldColor;
        private TransformComponent _RadVLTransform;
        private TransformComponent _RadVRTransform;
        private TransformComponent _RadHLTransform;
        private TransformComponent _RadHRTransform;
        private TransformComponent _LiftTransform;
        private TransformComponent _GabelTransform;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = AssetStorage.Get<SceneContainer>("gabelstapler.fus");
            _scenePicker = new ScenePicker(_scene);

            _RadVLTransform = _scene.Children.FindNodes(node => node.Name == "RadVL")?.FirstOrDefault()?.GetTransform();
            _RadVRTransform = _scene.Children.FindNodes(node => node.Name == "RadVR")?.FirstOrDefault()?.GetTransform();
            _RadHLTransform = _scene.Children.FindNodes(node => node.Name == "RadHL")?.FirstOrDefault()?.GetTransform();
            _RadHRTransform = _scene.Children.FindNodes(node => node.Name == "RadHR")?.FirstOrDefault()?.GetTransform();
            _LiftTransform = _scene.Children.FindNodes(node => node.Name == "Lift")?.FirstOrDefault()?.GetTransform();
            _GabelTransform = _scene.Children.FindNodes(node => node.Name == "Gabel")?.FirstOrDefault()?.GetTransform();

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            if (_camAngleVelocity > 0)
            {
                _camAngleVelocity -= 0.03f;
                if (_camAngleVelocity < 0)
                {
                    _camAngleVelocity = 0;
                }
            };
            if (_camAngleVelocity < 0)
            {
                _camAngleVelocity += 0.03f;
                if (_camAngleVelocity > 0)
                {
                    _camAngleVelocity = 0;
                }
            };

            if (Mouse.RightButton)
            {
                _camAngleVelocity = Mouse.Velocity.x / 10000;
            };

            _camAngle -= _camAngleVelocity;


            // Setup the camera 
            RC.View = float4x4.CreateTranslation(0, 0, 17) * float4x4.CreateRotationY(_camAngle);


            if (Mouse.LeftButton)
            {
                float2 pickPosClip = Mouse.Position * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);
                _scenePicker.View = RC.View;
                _scenePicker.Projection = RC.Projection;
                List<PickResult> pickResults = _scenePicker.Pick(pickPosClip).ToList();
                if (pickResults.Count > 0)
                {
                    Diagnostics.Log(pickResults[0].Node.Name);

                    PickResult newPick = null;
                    if (pickResults.Count > 0)
                    {
                        pickResults.Sort((a, b) => Sign(a.ClipPos.z - b.ClipPos.z));
                        newPick = pickResults[0];
                    }
                    if (newPick?.Node != _currentPick?.Node)
                    {
                        if (_currentPick != null)
                        {
                            _currentPick.Node.GetMaterial().Diffuse.Color = _oldColor;
                        }
                        if (newPick != null)
                        {
                            var mat = newPick.Node.GetMaterial();
                            _oldColor = mat.Diffuse.Color;
                            mat.Diffuse.Color = new float3(1, 0, 0);

                        }
                        _currentPick = newPick;
                    }
                }
            }

            if (_currentPick != null)
            {
                switch (_currentPick.Node.Name)
                {
                    case "RadVL":
                        float xRadVL = _RadVLTransform.Rotation.x;
                        xRadVL += Keyboard.UpDownAxis * 0.2f * (DeltaTime / 16 * 1000);
                        _RadVLTransform.Rotation = new float3(xRadVL, 0, 0);
                        break;

                    case "RadVR":
                        float xRadVR = _RadVRTransform.Rotation.x;
                        xRadVR += Keyboard.UpDownAxis * 0.2f * (DeltaTime / 16 * 1000);
                        _RadVRTransform.Rotation = new float3(xRadVR, 0, 0);
                        break;

                    case "RadHL":
                        float xRadHL = _RadHLTransform.Rotation.x;
                        float yRadHL = _RadHLTransform.Rotation.y;
                        xRadHL += Keyboard.UpDownAxis * 0.2f * (DeltaTime / 16 * 1000);
                        yRadHL += Keyboard.LeftRightAxis * -0.02f * (DeltaTime / 16 * 1000);
                        if (yRadHL < -0.25f) yRadHL = -0.25f;
                        if (yRadHL > 0.25f) yRadHL = 0.25f;
                        _RadHLTransform.Rotation = new float3(xRadHL, yRadHL, 0);
                        break;

                    case "RadHR":
                        float xRadHR = _RadHRTransform.Rotation.x;
                        float yRadHR = _RadHRTransform.Rotation.y;
                        xRadHR += Keyboard.UpDownAxis * 0.2f * (DeltaTime / 16 * 1000);
                        yRadHR += Keyboard.LeftRightAxis * -0.02f * (DeltaTime / 16 * 1000);
                        if (yRadHR < -0.25f) yRadHR = -0.25f;
                        if (yRadHR > 0.25f) yRadHR = 0.25f;
                        _RadHRTransform.Rotation = new float3(xRadHR, yRadHR, 0);
                        break;

                    case "Lift":
                        float xLift = _LiftTransform.Rotation.x;
                        xLift += Keyboard.LeftRightAxis * -0.005f * (DeltaTime / 16 * 1000);
                        if (xLift < -0.22f) xLift = -0.22f;
                        if (xLift > 0) xLift = 0;
                        _LiftTransform.Rotation = new float3(xLift, 0, 0);
                        break;

                    case "Gabel":
                        float yGabel = _GabelTransform.Translation.y;
                        yGabel += Keyboard.UpDownAxis * 0.05f * (DeltaTime / 16 * 1000);
                        if (yGabel < 0.03f) yGabel = 0.03f;
                        if (yGabel > 4) yGabel = 4;
                        _GabelTransform.Translation = new float3(_GabelTransform.Translation.x, yGabel, _GabelTransform.Translation.z);
                        break;
                }
            }


            // Render the scene on the current render context
            _sceneRenderer.Render(RC);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered farame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }
    }
}
