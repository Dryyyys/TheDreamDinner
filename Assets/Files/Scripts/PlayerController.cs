using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;

    private void Update()
    {
        if(!IsOwner) return;

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var direction = new Vector3(horizontal, 0, vertical);

        if (direction.sqrMagnitude > 0)
            MoveServerRpc(direction);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 direction)
    {
        transform.position += direction * _speed * Time.deltaTime;
    }
}
