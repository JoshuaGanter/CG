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
    public class Driving : RenderCanvas
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
        private TransformComponent _BaseTransform;
        private float _targetHeight;

        private TransformComponent _TrailerTransform;
        private float _d = 13;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = AssetStorage.Get<SceneContainer>("gabelstapler.fus");
            

            _RadVLTransform = _scene.Children.FindNodes(node => node.Name == "RadVL")?.FirstOrDefault()?.GetTransform();
            _RadVRTransform = _scene.Children.FindNodes(node => node.Name == "RadVR")?.FirstOrDefault()?.GetTransform();
            _RadHLTransform = _scene.Children.FindNodes(node => node.Name == "RadHL")?.FirstOrDefault()?.GetTransform();
            _RadHRTransform = _scene.Children.FindNodes(node => node.Name == "RadHR")?.FirstOrDefault()?.GetTransform();
            _LiftTransform = _scene.Children.FindNodes(node => node.Name == "Lift")?.FirstOrDefault()?.GetTransform();
            _GabelTransform = _scene.Children.FindNodes(node => node.Name == "Gabel")?.FirstOrDefault()?.GetTransform();
            _BaseTransform = _scene.Children.FindNodes(node => node.Name == "Basis")?.FirstOrDefault()?.GetTransform();

            _targetHeight = _GabelTransform.Translation.y;

            _TrailerTransform = new TransformComponent { Rotation = new float3(-M.Pi / 5.7f, 0, 0), Scale = float3.One, Translation = new float3(0, 0, -10) };

            _scene.Children.Add(new SceneNodeContainer
            {
                Components = new List<SceneComponentContainer>
                {
                    _TrailerTransform,
                    new MaterialComponent { Diffuse = new MatChannelContainer { Color = new float3(0.7f, 0.7f, 0.7f) }, Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }},
                    SimpleMeshes.CreateCuboid(new float3(2, 2, 2))
                }
            });


            _scenePicker = new ScenePicker(_scene);

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
            RC.View = float4x4.CreateRotationX(-M.Pi / 7.3f) * float4x4.CreateRotationY(M.Pi - _TrailerTransform.Rotation.y) * float4x4.CreateTranslation(-_TrailerTransform.Translation.x, - 6, -_TrailerTransform.Translation.z);


            if (Mouse.LeftButton)
            {
                float2 pickPosClip = Mouse.Position * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);
                _scenePicker.View = RC.View;
                _scenePicker.Projection = RC.Projection;
                List<PickResult> pickResults = _scenePicker.Pick(pickPosClip).ToList();
                if (pickResults.Count > 0)
                {
                    //Diagnostics.Log(pickResults[0].Node.Name);

                    PickResult newPick = null;
                    if (pickResults.Count > 0)
                    {
                        pickResults.Sort((a, b) => Sign(a.ClipPos.z - b.ClipPos.z));
                        newPick = pickResults[0];
                        if (!newPick.Node.Name.StartsWith("Cube")) newPick = null;
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

                            _targetHeight = newPick.Node.GetTransform().Translation.y - .8f;

                        }
                        _currentPick = newPick;
                    }
                }
                else
                {
                    if (_currentPick != null)
                    {
                        _currentPick.Node.GetMaterial().Diffuse.Color = _oldColor;
                        _currentPick = null;
                    }
                }
            }

            float xRadVL = _RadVLTransform.Rotation.x;
            xRadVL += Keyboard.WSAxis * 0.15f * (DeltaTime / 16 * 1000);
            _RadVLTransform.Rotation = new float3(xRadVL, 0, 0);
            
            float xRadVR = _RadVRTransform.Rotation.x;
            xRadVR += Keyboard.WSAxis * 0.15f * (DeltaTime / 16 * 1000);
            _RadVRTransform.Rotation = new float3(xRadVR, 0, 0);
            
            float xRadHL = _RadHLTransform.Rotation.x;
            float yRadHL = _RadHLTransform.Rotation.y;
            xRadHL += Keyboard.WSAxis * 0.15f * (DeltaTime / 16 * 1000);
            yRadHL = Keyboard.ADAxis * -0.35f;
            _RadHLTransform.Rotation = new float3(xRadHL, yRadHL, 0);

            float xRadHR = _RadHRTransform.Rotation.x;
            float yRadHR = _RadHRTransform.Rotation.y;
            xRadHR += Keyboard.WSAxis * 0.15f * (DeltaTime / 16 * 1000);
            yRadHR = Keyboard.ADAxis * -.35f;
            _RadHRTransform.Rotation = new float3(xRadHR, yRadHR, 0);

            float yGabel = _GabelTransform.Translation.y;
            if (_targetHeight < yGabel)
            {
                yGabel -= DeltaTime;
                if (_targetHeight > yGabel)
                {
                    yGabel = _targetHeight;
                }
            }

            if (_targetHeight > yGabel)
            {
                yGabel += DeltaTime;
                if (_targetHeight < yGabel)
                {
                    yGabel = _targetHeight;
                }
            }
            _GabelTransform.Translation = new float3(_GabelTransform.Translation.x, yGabel, _GabelTransform.Translation.z);

            /*float xLift = _LiftTransform.Rotation.x;
            xLift += Keyboard.LeftRightAxis * -0.005f * (DeltaTime / 16 * 1000);
            if (xLift < -0.22f) xLift = -0.22f;
            if (xLift > 0) xLift = 0;
            _LiftTransform.Rotation = new float3(xLift, 0, 0);
                        

                    
            float yGabel = _GabelTransform.Translation.y;
            yGabel += Keyboard.UpDownAxis * 0.05f * (DeltaTime / 16 * 1000);
            if (yGabel < 0.03f) yGabel = 0.03f;
            if (yGabel > 4) yGabel = 4;
            _GabelTransform.Translation = new float3(_GabelTransform.Translation.x, yGabel, _GabelTransform.Translation.z);
                      */


            float3 pAalt = _BaseTransform.Translation;
            float3 pBalt = _TrailerTransform.Translation;

            float posVel = Keyboard.WSAxis * Time.DeltaTime;
            float rotVel = Keyboard.ADAxis * Time.DeltaTime;

            float newRot = _BaseTransform.Rotation.y + (rotVel * Keyboard.WSAxis * Time.DeltaTime * 30);
            _BaseTransform.Rotation = new float3(0, newRot, 0);

            float3 pAneu = _BaseTransform.Translation + new float3(posVel * M.Sin(newRot) * 10, 0, posVel * M.Cos(newRot) * 10);
            _BaseTransform.Translation = pAneu;

            float3 pBneu = pAneu + (float3.Normalize(pBalt - pAneu) * _d);
            _TrailerTransform.Translation = pBneu;

            _TrailerTransform.Rotation = new float3(0, (float)System.Math.Atan2(float3.Normalize(pBalt - pAneu).x, float3.Normalize(pBalt - pAneu).z), 0);

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
