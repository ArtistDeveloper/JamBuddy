using Jambuddy.Adohi.Character.Hack;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public enum EventType
    {
        Add,
        Delete,
    }

    public class EffectTarget : MonoBehaviour
    {
        Dictionary<Type, Block> _blockDictionary = new Dictionary<Type, Block>();

        private bool isMovingOpposite;

        public void HandleBlockApplication(string blockType)
        {
            Debug.Log($"{gameObject.name} received block: {blockType}");
            
            switch (blockType)
            {
                case "OppositeMoving":
                    ApplyMoving();
                    break;
                case "Rotation":
                    ApplyRotation();
                    break;
                case "Gravity":
                    ApplyGravity();
                    break;
                case "Sizing":
                    ApplyIncreaseSize();
                    break;
            }

            ApplyEffect();
        }

        private void ApplyMoving()
        {
            if (_blockDictionary.ContainsKey(typeof(OppositeMoving)))
            {
                return;
            }

            if (isMovingOpposite)
            {
                Debug.LogWarning("이미 Oppsite Moving이 적용중임");
                return;
            }

            var block = new OppositeMoving();
            _blockDictionary[typeof(OppositeMoving)] = block;
        }

        private void ApplyGravity()
        {
            if (_blockDictionary.ContainsKey(typeof(Gravity)))
            {
                return;
            }

            var block = new Gravity();
            _blockDictionary[typeof(Gravity)] = block;
        }

        private void ApplyRotation()
        {
            if (_blockDictionary.ContainsKey(typeof(Rotation)))
            {
                return;
            }

            var block = new Rotation();
            _blockDictionary[typeof(Rotation)] = block;
        }

        private void ApplyIncreaseSize()
        {
            if (_blockDictionary.ContainsKey(typeof(Sizing)))
            {
                return;
            }

            var block = new Sizing();
            _blockDictionary[typeof(Sizing)] = block;
        }

        public void ApplyEffect()
        {
            foreach (var kvp in _blockDictionary)
            {
                kvp.Value.ApplyEffect(this);
                if (kvp.Value is OppositeMoving)
                {
                    OppositeMoving opMoving = kvp.Value as OppositeMoving;
                    isMovingOpposite = true;

                    opMoving.OnMoveOppositeComplete += () =>
                    {
                        isMovingOpposite = false;
                    };
                }
            }

            _blockDictionary = new Dictionary<Type, Block>();
        }
    }
}
