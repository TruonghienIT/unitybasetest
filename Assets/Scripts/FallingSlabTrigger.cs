using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public sealed class FallingSlabTrigger : MonoBehaviour
{
    [SerializeField] private Rigidbody fallingSlab;
    [SerializeField, Min(0f)] private float fallDelay = 1.5f;
    [SerializeField, Min(1f)] private float fallGravityMultiplier = 2.5f;

    private Collider _trigger;
    private Collider _slabCollider;
    private Renderer _slabRenderer;
    private bool _activated;

    private void FixedUpdate()
    {
        if (fallingSlab != null && !fallingSlab.isKinematic && fallingSlab.useGravity)
            fallingSlab.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
    }

    private void Awake()
    {
        _trigger = GetComponent<Collider>();

        if (fallingSlab == null)
        {
            enabled = false;
            return;
        }

        _slabCollider = fallingSlab.GetComponent<Collider>();
        _slabRenderer = fallingSlab.GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_activated || other.GetComponentInParent<CharacterController>() == null) return;

        _activated = true;
        _trigger.enabled = false;
        StartCoroutine(DropAfterDelay());
    }

    private IEnumerator DropAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, fallDelay * 0.5f));

        if (fallingSlab == null) yield break;

        fallingSlab.useGravity = true;
        fallingSlab.isKinematic = false;
    }
}
