﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceGame.units;
using SpaceGame.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.graphics
{
    class UnitSprite : Sprite
    {
        protected const string c_armSpritePath = c_unitSpritePath + "arms/";

        public static Dictionary<string, UnitSpriteData> UnitSpriteData;
        PhysicalUnit _unit;
        Texture2D _armTexture;
		//position of shoulder relative to unit center, shoulder relative to arm, and hand relative to arm
        Vector2 _unitShoulderOffset, _flippedUnitShoulderOffset, _armShoulderPos;
        Vector2 _handOffset;
        Vector2 _weaponOffset;

        public override int AnimationState
        {
            get
            {
                return base.AnimationState;
            }
            set
            {
                return;		//cannot set animation state on UnitSprite - determined by velocity
            }
        }

        protected UnitSprite(UnitSpriteData data, PhysicalUnit unit)
			:base(data, SpriteType.Unit)
        {
            _unit = unit;
            if (_numStates % 2 == 0)
            {
                throw new Exception(String.Format("UnitSprite {0} has an even number of states along the horizontal axis", data.Name));
            }
            _unitShoulderOffset = new Vector2(data.ShoulderX, data.ShoulderY);
            _flippedUnitShoulderOffset = new Vector2(-data.ShoulderX, data.ShoulderY);
            _armTexture = Content.Load<Texture2D>(c_armSpritePath + data.SpriteArmData.Name);
            _armShoulderPos = new Vector2(data.SpriteArmData.ShoulderX, data.SpriteArmData.ShoulderY);
            _handOffset = _unitShoulderOffset + new Vector2(data.SpriteArmData.HandX, data.SpriteArmData.HandY);
        }

        public UnitSprite(string name, PhysicalUnit unit)
            : this(UnitSpriteData[name], unit)
        { }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
			//update sprite based on unit's x velocity
            float velocityFactor = _unit.Velocity.X / _unit.MaxSpeed;

            if (!FlipH)	//if flipped, velocity response is opposite
            {
                velocityFactor *= -1;
            }

			//convert to positive scale
            velocityFactor = MathHelper.Clamp(velocityFactor, -1, 1) + 1;	//scale from 0 to 2 (1 being stationary)

			//set state based on velocity
            _currentState = (int)(velocityFactor / 2 * _numStates);

            _weaponOffset = _unit.WeaponSprite.HandleOffset + _handOffset;
        }

        public override void Draw(SpriteBatch batch, Vector2 position)
        {
            base.Draw(batch, position);
            float aimAngle = XnaHelper.RadiansFromVector(_unit.LookDirection);
            //draw arm
            if (FlipH)
            {
                batch.Draw(_armTexture, position + _flippedUnitShoulderOffset, null, Color.White, aimAngle, _armShoulderPos, Scale, SpriteEffects.None, 0);
            }
            else
            {
                batch.Draw(_armTexture, position + _unitShoulderOffset, null, Color.White, aimAngle, _armShoulderPos, Scale, SpriteEffects.None, 0);
            }
            //draw weapon
            if (_unit.WeaponSprite != null)
            {
                _unit.WeaponSprite.Draw(batch, position + _weaponOffset, aimAngle, _armShoulderPos);
            }
            XnaHelper.DrawRect(Color.Red, position + _unitShoulderOffset + _handOffset, 5, 5, batch);
        }

    }

    class UnitSpriteData : SpriteData
    {
        public int ShoulderX, ShoulderY; //where to anchor arm sprite
        public SpriteArmData SpriteArmData;
    }

	class SpriteArmData
    {
        public string Name;					//name of texture to use as arm
        public int ShoulderX, ShoulderY;	//place to anchor to unitsprite's shoulder
        public int HandX, HandY;		//place to anchor weapon handle
    }

}
