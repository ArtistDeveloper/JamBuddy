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
        List<Block> _blocks = new List<Block>();

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
                    IncreaseSize();
                    break;
            }
        }

        private void ApplyMoving()
        {
            _blocks.Add(new OppositeMoving());
        }

        private void ApplyGravity()
        {
            _blocks.Add(new Gravity());
        }

        private void ApplyRotation()
        {
            _blocks.Add(new Rotation());
        }

        private void IncreaseSize()
        {
            _blocks.Add(new Sizing());
        }

        public void ApplyEffect()
        {
            foreach (Block block in _blocks)
            {
                block.ApplyEffect(this);
            }
        }
    }
}
