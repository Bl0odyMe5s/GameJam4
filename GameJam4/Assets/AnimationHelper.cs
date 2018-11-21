using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper : MonoBehaviour {

    private Alien alien;

    private void Start()
    {
        alien = GetComponentInParent<Alien>();
    }

    public void EndAttackAnimation()
    {
        alien.EndAttack();
    }
}
