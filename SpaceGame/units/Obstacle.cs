﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SpaceGame.utility;
using SpaceGame.equipment;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.units
{
    class ObstacleData : PhysicalData
    {
        //spawn frequencies
        public float MinSpawnTime;
        public float MaxSpawnTime;
        public ProjectileEffectData DestroyEffect;
    }

    class Obstacle : PhysicalBody
    {
        float _minSpawnTime;
        float _maxSpawnTime;

        TimeSpan _respawnTimer;
        bool _isSpawned;
        Vector2 _destroyPosition;
        TimeSpan _explodeTimer;

        ProjectileEffect _destroyEffect;

        public Obstacle(ObstacleData data)
            :base(data, graphics.Sprite.SpriteType.Obstacle)
        {
            _minSpawnTime = data.MinSpawnTime;
            _maxSpawnTime = data.MaxSpawnTime;
            _respawnTimer = XnaHelper.RandomTime(_minSpawnTime, _maxSpawnTime);
            _destroyEffect = data.DestroyEffect == null ? ProjectileEffect.NullEffect : new ProjectileEffect(data.DestroyEffect);
            _explodeTimer = TimeSpan.Zero;
            _isSpawned = false;
        }

        public bool Exploding
        {
            get { return _explodeTimer > TimeSpan.Zero; }
        }

        public override float MassForBlackHole { get { return Mass * 0.25f; } }

        public override bool CanRespawn
        {
            get
            {
                return base.CanRespawn && _respawnTimer <= TimeSpan.Zero;
            }
        }

        public override void Update(GameTime gameTime, Rectangle levelBounds)
        {
            base.Update(gameTime, levelBounds);
            if (Exploding)
            {
                _destroyEffect.Update(gameTime);
                if (_explodeTimer > TimeSpan.Zero)
                {
                    _destroyEffect.SpawnParticles(gameTime.ElapsedGameTime, _destroyPosition, 0.0f, Vector2.Zero);
                    _explodeTimer -= gameTime.ElapsedGameTime;
                }
            }
            _destroyEffect.Update(gameTime);
        }

        public void UpdateRespawnTimer(TimeSpan time)
        {
            if (_lifeState == LifeState.Destroyed || _lifeState == LifeState.Dormant)
            {
                if (_isSpawned)
                {   //just destroyed
                    _isSpawned = false;
                    _respawnTimer = XnaHelper.RandomTime(_minSpawnTime, _maxSpawnTime);
                }
                _respawnTimer -= time;
            }
        }

        public void CheckAndApplyEffect(PhysicalBody other, TimeSpan time)
        {
            if (Exploding && _explodeTimer > TimeSpan.Zero)
            {
                _destroyEffect.TryApply(_destroyPosition, other, time);
                _explodeTimer -= time;
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            if (Updates)
                _destroyEffect.Draw(sb);
        }

        protected override void OnDisable()
        {
            if (_destroyEffect != ProjectileEffect.NullEffect)
            {
                _destroyPosition = _position;   //save position on destruction
                _explodeTimer = _destroyEffect.Duration;
            }
            shatter();
        }

        public override void Respawn(Vector2 newPosition)
        {
            base.Respawn(newPosition);
            _explodeTimer = TimeSpan.Zero;
        }
    }
}
