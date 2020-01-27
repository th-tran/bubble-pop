using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocker : Bubble
{
    public bool clearedByBomb = false;
    public bool clearedAtBottom = true;

    // Start is called before the first frame update
    void Start()
    {
        matchValue = MatchValue.None;
    }
}
