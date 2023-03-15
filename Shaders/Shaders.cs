using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Shaders
{
    public class Shaders : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SpriteFont font;
        private bool displayUI = true;
        private bool displayValues = true;

        #region - Variables -
        Effect effect;
        Model model;
        int currentShader = 0;
        string currentShaderType = "Gourand Vertex";

        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        Vector4 ambient = new Vector4(0, 0, 0, 0);
        Vector4 diffuseColor = new Vector4(1, 1, 1, 1);
        Vector3 lightPosition = new Vector3(1, 1, 1);
        float ambientIntensity = 0;
        float diffuseIntensity = 1f;
        float specularIntensity = 1f;
        float shininess = 20f;

        MouseState preMouseState;
        KeyboardState preKeyboardState;

        float angleY;
        float xSensitivity = 0.01f;
        float ySensitivity = 0.01f;
        float angleX;

        float xPosition = 0;
        float yPosition = 0;
        float distance = 2f;

        float lightAngleX = 0f;
        float lightAngleY = 0f;

        Matrix view;
        Matrix world;
        Matrix projection;
        Vector3 cameraPosition;

        #endregion 

        public Shaders()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            model = Content.Load<Model>("Box");
            effect = Content.Load<Effect>("Shaders");
            font = Content.Load<SpriteFont>("UI");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            bool shift = false;
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            foreach (Keys key in pressedKeys)
                if (key == Keys.LeftShift || key == Keys.RightShift) shift = true;

            #region - Basic User Interfaces -

            #region - Camera Rotate -
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();
            if (currentMouseState.LeftButton == ButtonState.Pressed && preMouseState.LeftButton == ButtonState.Pressed)
            {
                angleX += (preMouseState.X - currentMouseState.X) * xSensitivity;
                angleY += (preMouseState.Y - currentMouseState.Y) * ySensitivity;
            }
            #endregion

            #region - Camera Distance -
            if (currentMouseState.RightButton == ButtonState.Pressed && preMouseState.RightButton == ButtonState.Pressed)
            {
                distance += (preMouseState.Y - currentMouseState.Y) / 2f;
            }
            #endregion

            #region - Camera Translate -
            if (currentMouseState.MiddleButton == ButtonState.Pressed && preMouseState.MiddleButton == ButtonState.Pressed)
            {
                xPosition += (preMouseState.X - currentMouseState.X) / 2f;
                yPosition += (preMouseState.Y - currentMouseState.Y) / 2f;
            }
            #endregion

            #region - Light Rotate -
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && currentKeyboardState.GetPressedKeys().Length == 1)
            {
                lightAngleX -= .01f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                lightAngleX += .01f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                lightAngleY += .01f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                lightAngleY -= .01f;
            }
            #endregion

            #region - Reset Settings -
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                xPosition = 0;
                yPosition = 0;
                distance = 20f;
                angleX = 0;
                angleY = 0;
                lightAngleX = 0;
                lightAngleY = 0;
                diffuseIntensity = 1f;
                diffuseColor = new Vector4(1, 1, 1, 1);
            }
            #endregion

            #endregion

            #region - Geometry Loader -

            // The models provided under the "assignment one" are smaller than
            // the models provided under the previous labs, so this is why it appears that way.
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) // Box
            {
                model = Content.Load<Model>("Box");
                distance = 2f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) // Sphere
            {
                model = Content.Load<Model>("Sphere");
                distance = 2f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) // Tea Pot
            {
                model = Content.Load<Model>("Teapot");
                distance = 2f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) // Torus
            {
                model = Content.Load<Model>("Torus");
                distance = 20f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) // Bunny
            {
                model = Content.Load<Model>("Bunny");
                distance = 20f;
            }

            #endregion

            #region - Shader/Lighting Models -

            #region - Shader Effects -
            if (Keyboard.GetState().IsKeyDown(Keys.F1)) // Gourand Vertex
            {
                currentShader = 0;
                currentShaderType = "Gourand Vertex";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) // Phong Pixel
            {
                currentShader = 1;
                currentShaderType = "Phong Pixel";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) // PhongBlinn 
            {
                currentShader = 2;
                currentShaderType = "PhongBlinn";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) // Schlick 
            {
                currentShader = 3;
                currentShaderType = "Schlick";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F5)) // Toon 
            {
                currentShader = 4;
                currentShaderType = "Toon";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F6)) // HalfLife 
            {
                currentShader = 5;
                currentShaderType = "HalfLife";
            }
            #endregion

            #region - Light Properties -
            if (Keyboard.GetState().IsKeyDown(Keys.RightShift))
            {
                diffuseIntensity += .01f;
                diffuseColor.X += .01f;
                diffuseColor.Y += .01f;
                diffuseColor.Z += .01f;

            }
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                diffuseIntensity -= .01f;
                diffuseColor.X -= .01f;
                diffuseColor.Y -= .01f;
                diffuseColor.Z -= .01f;

            }
            #endregion

            #region - Specular Intensity - 
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus) && currentKeyboardState.GetPressedKeys().Length == 1)
            {
                // Increases the intensity
                specularIntensity += .1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) && currentKeyboardState.GetPressedKeys().Length == 1)
            {
                // Decreases the intensity
                specularIntensity -= .1f;
            }
            #endregion

            #region - Shininess - 
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                // Increase Shininess
                shininess += .1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                // Decrease Shininess
                shininess -= .1f;
            }
            #endregion

            #region - Text Information -

            if (Keyboard.GetState().IsKeyDown(Keys.OemQuestion) && !preKeyboardState.IsKeyDown(Keys.OemQuestion))
            {
                //  Show/hide the help screen showing the information of key/mouse controls
                displayUI = !displayUI;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.H) && !preKeyboardState.IsKeyDown(Keys.H))
            {
                // Show/hide all information used in the shader such as camera angle, light angle, shadertype, intensity, specular, rgb values of light, etc.
                displayValues = !displayValues;
            }
            #endregion

            #endregion

            #region - Transformations - 
            lightPosition = Vector3.Transform(
                new Vector3(1, 1, 1),
                Matrix.CreateRotationX(lightAngleY) * Matrix.CreateRotationY(lightAngleX));

            cameraPosition = Vector3.Transform(
                new Vector3(xPosition, yPosition, distance),
                Matrix.CreateRotationX(angleY) * Matrix.CreateRotationY(angleX));
            view = Matrix.CreateLookAt(
                cameraPosition,
                new Vector3(xPosition, yPosition, 0),
                Vector3.Transform(Vector3.Up, Matrix.CreateRotationX(angleY) * Matrix.CreateRotationY(angleX)));
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.33f, 0.1f, 100);
            #endregion



            preMouseState = currentMouseState;
            preKeyboardState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque; // BUG: Causes the Objects to render weirdley


            // TODO: Add your drawing code here

            effect.CurrentTechnique = effect.Techniques[currentShader];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // ?? Where is this data going ??
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["AmbientColor"].SetValue(ambient);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);
                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);

                        Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                        // Lab04
                        effect.Parameters["SpecularColor"].SetValue(specularColor);
                        effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                        effect.Parameters["Shininess"].SetValue(shininess);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        pass.Apply();
                        // ?? What is VertexBuffer, IndexBuffer ??
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount
                            );
                    }
                }
            }

            if (displayUI)
            {
                _spriteBatch.Begin();
                _spriteBatch.DrawString(font, "CONTROLS", new Vector2(25, 25), Color.Black);
                _spriteBatch.DrawString(font, "Toggle Controls: ?", new Vector2(25, 45), Color.Black);
                _spriteBatch.DrawString(font, "Rotate the camera : Mouse Left Drag", new Vector2(25, 65), Color.Black);
                _spriteBatch.DrawString(font, "Change the distance of camera to the center: Mouse Right Drag", new Vector2(25, 85), Color.Black);
                _spriteBatch.DrawString(font, "Translate the camera: Mouse Middle Drag", new Vector2(25, 105), Color.Black);
                _spriteBatch.DrawString(font, "Rotate the light: Arrow keys", new Vector2(25, 125), Color.Black);
                _spriteBatch.DrawString(font, "Reset camera and light: S ", new Vector2(25, 145), Color.Black);
                _spriteBatch.DrawString(font, "Change Shader: F1-F6", new Vector2(25, 165), Color.Black);
                _spriteBatch.DrawString(font, "Change Object: 1-5", new Vector2(25, 185), Color.Black);
                _spriteBatch.DrawString(font, "RGB properties: Shift", new Vector2(25, 205), Color.Black);
                _spriteBatch.DrawString(font, "Specular Intensity: +/-", new Vector2(25, 225), Color.Black);
                _spriteBatch.DrawString(font, "Shininess: Left Arrow & +/-", new Vector2(25, 245), Color.Black);
                _spriteBatch.DrawString(font, "Information: H", new Vector2(25, 265), Color.Black);
                _spriteBatch.End();
            }

            if (displayValues)
            {
                _spriteBatch.Begin();
                _spriteBatch.DrawString(font, "VALUES", new Vector2(525, 25), Color.Black);
                _spriteBatch.DrawString(font, "Camera Angle: (" + (int)(angleX * 100) + "," + (int)(angleY * 100) + ")", new Vector2(525, 45), Color.Black);
                _spriteBatch.DrawString(font, "Light Angle: (" + (int)(lightAngleX * 100) + "," + (int)(lightAngleY * 100) + ")", new Vector2(525, 65), Color.Black);
                _spriteBatch.DrawString(font, "Shader Type: " + currentShaderType, new Vector2(525, 85), Color.Black);
                _spriteBatch.DrawString(font, "Light Intensity: " + diffuseIntensity.ToString("0.00"), new Vector2(525, 105), Color.Black);
                _spriteBatch.DrawString(font, "R: " + diffuseColor.W.ToString("0.00"), new Vector2(525, 125), Color.Black);
                _spriteBatch.DrawString(font, "G: " + diffuseColor.Y.ToString("0.00"), new Vector2(525, 145), Color.Black);
                _spriteBatch.DrawString(font, "B: " + diffuseColor.Z.ToString("0.00"), new Vector2(525, 165), Color.Black);
                _spriteBatch.DrawString(font, "Specular Intensity: " + specularIntensity.ToString("0.00"), new Vector2(525, 185), Color.Black);
                _spriteBatch.DrawString(font, "Shininess: " + shininess.ToString("0.00"), new Vector2(525, 205), Color.Black);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}