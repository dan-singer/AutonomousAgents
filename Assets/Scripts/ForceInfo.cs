using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SForceInfo  {
    public float weight;
}
[System.Serializable]
public class SForceRadiusInfo : SForceInfo{ 
    public float radius;
}
[System.Serializable]
public class PursueEvadeInfo : SForceInfo{
    public float secondsAhead;
}
[System.Serializable]
public class WanderInfo: SForceRadiusInfo{
    public float unitsAhead;
}




