#pragma once

#include "PoseTracker.h"
#include "GameFramework/Actor.h"
#include "RobotTracker.generated.h"

UCLASS()
class TREASUREHUNT_API ARobotTracker : public APoseTracker
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	ARobotTracker();

	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void Tick( float DeltaSeconds ) override;
    
    // Shows or hides circle outlining around Cozmo
    UFUNCTION()
    void ShowOutline(bool shouldShow);
    
protected:
    // Fetches tracked pose from Cozmo SDK via CozmoUE object. This should be implemented in subclasses (so RobotTracker
    // could get Cozmo's pose, CubeTracker could get cube's pose, and so on).
    virtual FCozmoPoseStruct FetchPose() override;
    
private:
    UStaticMeshComponent *_outline;
};
