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
    public class HierarchyInput : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private float _camAngle = 0;
        private float _camAngleIncrement = 0;
        private float _grabRotation = 0.5f;
        private Boolean _grabClosed = false;
        private TransformComponent _bodyTransform;
        private TransformComponent _upperArmTransform;
        private TransformComponent _lowerArmTransform;
        private TransformComponent _grabATransform;
        private TransformComponent _grabBTransform;

        SceneContainer CreateScene()
        {
            // Initialize transform components that need to be changed inside "RenderAFrame"
            _bodyTransform = new TransformComponent { Rotation = float3.Zero, Scale = float3.One, Translation = new float3(0, 6, 0) };
            _upperArmTransform = new TransformComponent { Rotation = float3.Zero, Scale = float3.One, Translation = new float3(1, 4, 0) };
            _lowerArmTransform = new TransformComponent { Rotation = float3.Zero, Scale = float3.One, Translation = new float3(-1, 4, 0) };
            _grabATransform = new TransformComponent { Rotation = new float3(-0.5f, 0, 0), Scale = float3.One, Translation = new float3(0, 5, -0.5f) };
            _grabBTransform = new TransformComponent { Rotation = new float3(0.5f, 0, 0), Scale = float3.One, Translation = new float3(0, 5, 0.5f) };

            // Setup the scene graph
            return new SceneContainer
            {
                Children = new List<SceneNodeContainer>
                {
                    // Base
                    new SceneNodeContainer
                    {
                        Components = new List<SceneComponentContainer>
                        {
                            new TransformComponent { Rotation = float3.Zero, Scale = float3.One, Translation = float3.Zero },
                            new MaterialComponent { Diffuse = new MatChannelContainer { Color = new float3(0.7f, 0.7f, 0.7f) }, Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }},
                            SimpleMeshes.CreateCuboid(new float3(10, 2, 10))
                        },
                        Children = new List<SceneNodeContainer>
                        {
                            // Body
                            new SceneNodeContainer
                            {
                                Components = new List<SceneComponentContainer>
                                {
                                    _bodyTransform,
                                    new MaterialComponent { Diffuse = new MatChannelContainer { Color = new float3(1, 0, 0) }, Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }},
                                    SimpleMeshes.CreateCuboid(new float3(2, 10, 2))
                                },
                                Children = new List<SceneNodeContainer>
                                {
                                    // Upper Arm Joint
                                    new SceneNodeContainer
                                    {
                                        Components = new List<SceneComponentContainer>
                                        {
                                            _upperArmTransform
                                        },
                                        Children = new List<SceneNodeContainer>
                                        {
                                            // Upper Arm
                                            new SceneNodeContainer
                                            {
                                                Components = new List<SceneComponentContainer>
                                                {
                                                    new TransformComponent { Rotation = float3.Zero, Scale = float3.One, Translation = new float3(1, 4, 0) },
                                                    new MaterialComponent { Diffuse = new MatChannelContainer { Color = new float3(0, 1, 0) }, Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }},
                                                    SimpleMeshes.CreateCuboid(new float3(2, 10, 2))
                                                },
                                                Children = new List<SceneNodeContainer>
                                                {
                                                    // Lower Arm Joint
                                                    new SceneNodeContainer
                                                    {
                                                        Components = new List<SceneComponentContainer>
                                                        {
                                                            _lowerArmTransform
                                                        },
                                                        Children = new List<SceneNodeContainer>
                                                        {
                                                            // Lower Arm
                                                            new SceneNodeContainer
                                                            {
                                                                Components = new List<SceneComponentContainer>
                                                                {
                                                                    new TransformComponent { Rotation = float3.Zero, Scale = float3.One, Translation = new float3(-1, 4, 0) },
                                                                    new MaterialComponent { Diffuse = new MatChannelContainer { Color = new float3(0, 0, 1) }, Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }},
                                                                    SimpleMeshes.CreateCuboid(new float3(2, 10, 2))
                                                                },
                                                                Children = new List<SceneNodeContainer>
                                                                {
                                                                    // Grab 1 Joint
                                                                    new SceneNodeContainer
                                                                    {
                                                                        Components = new List<SceneComponentContainer>
                                                                        {
                                                                            _grabATransform
                                                                        },
                                                                        Children = new List<SceneNodeContainer>
                                                                        {
                                                                            // Grab 1
                                                                            new SceneNodeContainer
                                                                            {
                                                                                Components = new List<SceneComponentContainer>
                                                                                {
                                                                                    new TransformComponent { Rotation = float3.Zero, Scale = float3.One, Translation = new float3(0, 1, 0) },
                                                                                    new MaterialComponent { Diffuse = new MatChannelContainer { Color = new float3(1, 1, 1) }, Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }},
                                                                                    SimpleMeshes.CreateCuboid(new float3(1.8f, 3, 0.5f))
                                                                                }
                                                                            }
                                                                        }
                                                                    },
                                                                    // Grab 2 Joint
                                                                    new SceneNodeContainer
                                                                    {
                                                                        Components = new List<SceneComponentContainer>
                                                                        {
                                                                            _grabBTransform
                                                                        },
                                                                        Children = new List<SceneNodeContainer>
                                                                        {
                                                                            // Grab 2
                                                                            new SceneNodeContainer
                                                                            {
                                                                                Components = new List<SceneComponentContainer>
                                                                                {
                                                                                    new TransformComponent { Rotation = float3.Zero, Scale = float3.One, Translation = new float3(0, 1, 0) },
                                                                                    new MaterialComponent { Diffuse = new MatChannelContainer { Color = new float3(1, 1, 1) }, Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }},
                                                                                    SimpleMeshes.CreateCuboid(new float3(1.8f, 3, 0.5f))
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = CreateScene();

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Animate the body
            _bodyTransform.Rotation = new float3(0, _bodyTransform.Rotation.y + (Keyboard.LeftRightAxis * 0.05f * DeltaTime / 16 * 1000), 0);

            // Animate the upper arm
            _upperArmTransform.Rotation = new float3(_upperArmTransform.Rotation.x + (Keyboard.UpDownAxis * 0.05f * DeltaTime / 16 * 1000), 0, 0);

            // Animate the lower Arm
            _lowerArmTransform.Rotation = new float3(_lowerArmTransform.Rotation.x + (Keyboard.WSAxis * 0.05f * DeltaTime / 16 * 1000), 0, 0);

            // Animate the grabs
            _grabATransform.Rotation = new float3(_grabATransform.Rotation.x + (Keyboard.ADAxis * 0.05f * DeltaTime / 16 * 1000), 0, 0);
            _grabBTransform.Rotation = new float3(_grabBTransform.Rotation.x - (Keyboard.ADAxis * 0.05f * DeltaTime / 16 * 1000), 0, 0);

            // Limit the grab's rotation
            if (_grabATransform.Rotation.x > 0.1083f) _grabATransform.Rotation.x = 0.1083f;
            if (_grabATransform.Rotation.x < -0.5f) _grabATransform.Rotation.x = -0.5f;
            if (_grabBTransform.Rotation.x < -0.1083f) _grabBTransform.Rotation.x = -0.1083f;
            if (_grabBTransform.Rotation.x > 0.5f) _grabBTransform.Rotation.x = 0.5f;

            // Switch the state of the grabs with the f key, only works on desktop export..
            //if (Keyboard.IsKeyDown(KeyCodes.F))
            //{
            //    _grabClosed = (!_grabClosed);
            //}

            //if (_grabClosed && (_grabRotation != -0.1083f))
            //{
            //    _grabRotation -= 0.01f * (DeltaTime / 16 * 1000);
            //    if (_grabRotation < -0.1083f) _grabRotation = -0.1083f;
            //}
            //if ((!_grabClosed) && (_grabRotation != 0.5f))
            //{
            //    _grabRotation += 0.01f * (DeltaTime / 16 * 1000);
            //    if (_grabRotation > 0.5f) _grabRotation = 0.5f;
            //}
            //_grabATransform.Rotation = new float3(-_grabRotation, 0, 0);
            //_grabBTransform.Rotation = new float3(_grabRotation, 0, 0);

            // Animate the camera angle, swipe effect
            if (_camAngleIncrement > 0)
            {
                _camAngleIncrement -= 0.01f;
                if (_camAngleIncrement <= 0) _camAngleIncrement = 0;
            }
            else if(_camAngleIncrement < 0)
            {
                _camAngleIncrement += 0.01f;
                if (_camAngleIncrement >= 0) _camAngleIncrement = 0;
            }
            
            if(Mouse.LeftButton) _camAngleIncrement = Mouse.Velocity.x / 10000;

            _camAngle -= _camAngleIncrement;

            // Setup the camera 
            RC.View = float4x4.CreateTranslation(0, -10, 50) * float4x4.CreateRotationY(_camAngle);

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