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

        private void Awake()
        {
            SubscribeEvent(EventType.Add, HandleBlockApplication);
        }

        // 추후 외부에서 호출해서 Target에 대한 Event를 등록할 수 있음
        public void SubscribeEvent(EventType evtTYpe, Action<string> action)
        {
            switch (evtTYpe)
            {
                case EventType.Add:
                    EffectTargetManager.onAddBlock += action;
                    EffectTargetManager.onApplyEffect += ApplyEffect;
                    break;
                default:
                    break;
            }
        }

        public void HandleBlockApplication(string blockType)
        {
            Debug.Log($"{gameObject.name} received block: {blockType}");
            
            switch (blockType)
            {
                case "gravity":
                    ApplyGravity();
                    break;
                case "rotation":
                    ApplyRotation();
                    break;         
            }
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

        }

        private void ApplyEffect()
        {
            foreach (Block block in _blocks)
            {
                block.ApplyEffect(this);
            }
        }
    }
}
