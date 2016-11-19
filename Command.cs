using UnityEngine;
using System.Collections;

public abstract class Command //: Monobehaviour
{
	abstract public void execute(Character actor);
}