using UnityEngine;

public abstract class AnimatorKey
{
    public static class Parameter
    {
        #region ===== Int =====

        //public const string AttackIndex = "AttackIndex";

        #endregion ===== Int =====

        #region ===== Float =====

        //public const string Speed = "Speed";

        #endregion ===== Float =====

        #region ===== Bool =====

        //public const string IsDash = "IsDash";

        #endregion ===== Bool =====

        #region ===== Trigger =====

        //public const string DoIdleChange = "DoIdleChange";

        #endregion ===== Trigger =====
    }

    public static class Name
    {
        //public const string Idle = "Idle";
    }

    public static class Layer
    {
        //public const string BaseLayer = "BaseLayer";
    }

    public static class Hash
    {
        #region ===== Int =====

        //public static readonly int AttackIndex = Animator.StringToHash(Parameter.AttackIndex);

        #endregion ===== Int =====

        #region ===== Float =====

        //public static readonly int Speed = Animator.StringToHash(Parameter.Speed);

        #endregion ===== Float =====

        #region ===== Bool =====

        //public static readonly int IsDash = Animator.StringToHash(Parameter.IsDash);

        #endregion ===== Bool =====

        #region ===== Trigger =====

        //public static readonly int DoIdleChange = Animator.StringToHash(Parameter.DoIdleChange);

        #endregion ===== Trigger =====

        #region ===== State =====

        //public static readonly int Idle = Animator.StringToHash(Name.Idle);

        #endregion ===== State =====
    }
}