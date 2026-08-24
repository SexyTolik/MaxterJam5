using System;
using InteractSystem;
using PasswordSystem;
using UnityEngine;

namespace ComputerSystem
{
    public class Computer : MonoBehaviour, IInteractable
    {
        [SerializeField] private ComputerView _view;
        [SerializeField] private ClickToMoveController Player;
        [SerializeField] private float MinDistance = 4f;


        public void Interact()
        {
            if(Vector3.Distance(transform.position, Player.transform.position) > MinDistance)
            {
                Player.MoveTo(transform.position);
                return;
            }
            _view.Show();
        }
    }
}