using UnityEngine;
using System.Collections;

public class ModifyAnimParam : StateMachineBehaviour 
{
    public enum Timing { OnEnter, OnExit }
    public enum Modification { Set, Add, Subtract, Multiply, Divide, Toggle, Reset }
    public enum Type { Float, Int, Bool, Trigger }


    [SerializeField]
    string inChild;
    [SerializeField]
    Timing triggerEvent;

    [Space]
    
    [SerializeField]
    string paramName;
    [SerializeField]
    Type paramType;
    [SerializeField]
    Modification modifier;
    [SerializeField]
    float floatValue;
    [SerializeField]
    int intValue;
    [SerializeField]
    bool boolValue;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    {
        if (triggerEvent != Timing.OnEnter)
            return;

        if (inChild == "")
            Modify(animator);
        else
            Modify(animator.transform.FindChild(inChild).GetComponent<Animator>());
    }

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (triggerEvent != Timing.OnExit)
            return;

        if (inChild == "")
            Modify(animator);
        else
            Modify(animator.transform.FindChild(inChild).GetComponent<Animator>());
    }

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

    void Modify(Animator animator)
    {
        if (animator == null)
            Debug.LogError("No animator found in target child");

        int id = Animator.StringToHash(paramName);

        switch (paramType)
        {
            case Type.Float:
                switch (modifier)
                {
                    case Modification.Set:
                        animator.SetFloat(id, floatValue);
                        break;
                    case Modification.Add:
                        animator.SetFloat(id, animator.GetFloat(id) + floatValue);
                        break;
                    case Modification.Subtract:
                        animator.SetFloat(id, animator.GetFloat(id) - floatValue);
                        break;
                    case Modification.Multiply:
                        animator.SetFloat(id, animator.GetFloat(id) * floatValue);
                        break;
                    case Modification.Divide:
                        animator.SetFloat(id, animator.GetFloat(id) / floatValue);
                        break;
                }
                break;

            case Type.Int:
                switch (modifier)
                {
                    case Modification.Set:
                        animator.SetInteger(id, intValue);
                        break;
                    case Modification.Add:
                        animator.SetInteger(id, animator.GetInteger(id) + intValue);
                        break;
                    case Modification.Subtract:
                        animator.SetInteger(id, animator.GetInteger(id) - intValue);
                        break;
                    case Modification.Multiply:
                        animator.SetInteger(id, animator.GetInteger(id) * intValue);
                        break;
                    case Modification.Divide:
                        animator.SetInteger(id, animator.GetInteger(id) / intValue);
                        break;
                }
                break;
                
            case Type.Bool:
                switch (modifier)
                {
                    case Modification.Set:
                        animator.SetBool(id, boolValue);
                        break;
                    case Modification.Toggle:
                        animator.SetBool(id, !animator.GetBool(id));
                        break;
                }
                break;

            case Type.Trigger:
                switch (modifier)
                {
                    case Modification.Set:
                        animator.SetTrigger(id);
                        break;
                    case Modification.Reset:
                        animator.ResetTrigger(id);
                        break;
                }
                break;
        }
    }
}
