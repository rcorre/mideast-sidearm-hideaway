using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

using SpaceGame.graphics;
using SpaceGame.units;
using SpaceGame.utility;
using SpaceGame.equipment;
using SpaceGame.states;

using MonoGameExtensions;


namespace SpaceGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region const
        public const int SCREENWIDTH = 1280;
        public const int SCREENHEIGHT = 720;
        #endregion
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        InputManager _inputManager = new InputManager();
        InventoryManager _weaponManager = new InventoryManager();
        List<Gamestate> _stateStack = new List<Gamestate>();  
        TimeSpan _initTimer; 	//hack to set window size properly (see update)

        public static GameStates gamestate;

        public enum GameStates
        {
            Menu,
            Running,
            GameOver
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //set resolution TODO: Move this to an XML Configuration File
            graphics.PreferredBackBufferWidth = SCREENWIDTH;
            graphics.PreferredBackBufferHeight = SCREENHEIGHT;
            
            //TODO: replace with custom cursor
            this.IsMouseVisible = false;

			_initTimer = TimeSpan.FromSeconds(1.0);

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Window.SetPosition(new Point(0, 0));
            _inputManager = new InputManager();
            _weaponManager = new InventoryManager();
            gamestate = GameStates.Menu;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Initialize blank particle texture as a single pixel
            ParticleGenerator.ParticleTexture = new Texture2D(GraphicsDevice, 1, 1);
            ParticleGenerator.ParticleTexture.SetData<Color>(new Color[] {Color.White});

            //load hud textures
            //SpaceGame.graphics.hud.RadialBar.BarPipTexture = Content.Load<Texture2D>("hud/radial_bar_pip");
            SpaceGame.graphics.hud.RadialBar.GameGraphicsDevice = GraphicsDevice;

            //Load GUI textures
            SpaceGame.graphics.hud.Hud.targetWheel = Content.Load<Texture2D>("gui/Minimap_&_Health_Bar_00");
            SpaceGame.graphics.hud.Hud.leftClick = Content.Load<Texture2D>("gui/Left Click");
            SpaceGame.graphics.hud.Hud.rightClick = Content.Load<Texture2D>("gui/Right Click");
            SpaceGame.graphics.hud.Hud.spaceClick = Content.Load<Texture2D>("gui/Space Bar");
            SpaceGame.graphics.hud.Hud.shiftClick = Content.Load<Texture2D>("gui/Shift");
            SpaceGame.graphics.hud.Hud.button1 = Content.Load<Texture2D>("gui/Numer 1");
            SpaceGame.graphics.hud.Hud.button3 = Content.Load<Texture2D>("gui/Numer 3");
            SpaceGame.graphics.hud.Hud.button5 = Content.Load<Texture2D>("gui/Numer 5");
            SpaceGame.graphics.hud.Hud.button2 = Content.Load<Texture2D>("gui/Numer 2");
            SpaceGame.graphics.hud.Hud.button4 = Content.Load<Texture2D>("gui/Numer 4");
            SpaceGame.graphics.hud.Hud.button6 = Content.Load<Texture2D>("gui/Numer 6");
            SpaceGame.graphics.hud.Hud.voidWheel = Content.Load<Texture2D>("gui/Score_&_Void_Tracker");

			Level.s_CursorTexture = Content.Load<Texture2D>("gui/Cross_Hair_Basic_Green");

            Sprite.IceCubeTexture = Content.Load<Texture2D>("spritesheets/IceCube");

            XnaHelper.PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            XnaHelper.PixelTexture.SetData<Color>(new Color[] {Color.White});

            Sprite.Content = Content;   //Sprite gets reference to content so it can load textures
            ParticleGenerator.Content = Content;   //ParticleGenerator gets reference to content so it can load textures
            Gamestate.Content = Content;

            Gamemenu.LoadContent(Content);
            _stateStack.Add(new Gamemenu(Content));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //hack to set window size properly in Linux - set during first update
			if (_initTimer > TimeSpan.Zero) {
				graphics.PreferredBackBufferWidth = 1280;
				graphics.PreferredBackBufferHeight = 720;
				graphics.IsFullScreen = false;
				graphics.ApplyChanges();
				_initTimer -= gameTime.ElapsedGameTime;
			}

            if (_inputManager.Exit)
                this.Exit();

            _inputManager.Update();
            _stateStack.Last().Update(gameTime, _inputManager, _weaponManager);

            if (_stateStack.Last().ReplaceState != null)
            {
                Gamestate newState = _stateStack.Last().ReplaceState;
                _stateStack.RemoveAt(_stateStack.Count - 1);
                _stateStack.Add(newState);
                _inputManager.SetCameraOffset(Vector2.Zero);    //reset inputmanager camera offset
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
  
            //spriteBatch.Begin();
            _stateStack.Last().Draw(spriteBatch);
            //spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
