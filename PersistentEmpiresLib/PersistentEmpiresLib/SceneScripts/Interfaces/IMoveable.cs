using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts.Interfaces
{
    public interface IMoveable
    {
        UsableMachine GetAttachedObject();
        // void SetFrameAfterTick(MatrixFrame frame);


        float GetAdvanceSpeed();
        float GetRotationSpeed();
        float GetElevationSpeed();
        bool GetCanAdvance();
        bool GetCanRotate();
        bool GetCanElevate();
        bool GetAlwaysAlignToTerritory();
        Agent GetPilotAgent();


        bool IsMovingForward { get; set; }
        bool IsMovingBackward { get; set; }
        bool IsTurningRight { get; set; }
        bool IsTurningLeft { get; set; }
        bool IsMovingUp { get; set; }
        bool IsMovingDown { get; set; }
    }
}
