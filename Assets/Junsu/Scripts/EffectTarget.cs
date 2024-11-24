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
        }

        private void ApplyMoving()
        {
            if (_blockDictionary.ContainsKey(typeof(OppositeMoving)))
            {
                Debug.LogWarning("OppositeMoving block already exists.");
                return;
            }

            var block = new OppositeMoving();
            _blockDictionary[typeof(OppositeMoving)] = block;
        }

        private void ApplyGravity()
        {
            if (_blockDictionary.ContainsKey(typeof(Gravity)))
            {
                Debug.LogWarning("Gravity block already exists.");
                return;
            }

            var block = new Gravity();
            _blockDictionary[typeof(Gravity)] = block;
        }

        private void ApplyRotation()
        {
            if (_blockDictionary.ContainsKey(typeof(Rotation)))
            {
                Debug.LogWarning("Rotation block already exists.");
                return;
            }

            var block = new Rotation();
            _blockDictionary[typeof(Rotation)] = block;
        }

        private void ApplyIncreaseSize()
        {
            if (_blockDictionary.ContainsKey(typeof(Sizing)))
            {
                Debug.LogWarning("Sizing block already exists.");
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
            }
        }
    }
}
