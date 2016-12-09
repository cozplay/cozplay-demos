#pragma once

#include "UnrealEnginePython/Private/UnrealEnginePythonPrivatePCH.h"
#include "UnrealEnginePython/Public/PythonComponent.h"
#include "GameFramework/Actor.h"
#include "CozmoUE.generated.h"

USTRUCT()
struct TREASUREHUNT_API FCozmoPoseStruct
{
    GENERATED_BODY()
    
    FCozmoPoseStruct()
    {
        originID = 0;
        position = FVector::ZeroVector;
        rotation = FQuat::Identity;
        zAngleRadians = 0;
        zAngleDegrees = 0;
    }
    
    FCozmoPoseStruct(const TArray<FString> &poseStrings)
    {
        if (poseStrings.Num() == 0) {
            FCozmoPoseStruct();
        } else {
            originID = FCString::Atoi(*poseStrings[0]);
            position = FVector(FCString::Atof(*poseStrings[1]),
                               FCString::Atof(*poseStrings[2]),
                               FCString::Atof(*poseStrings[3]));
            rotation = FQuat(FCString::Atof(*poseStrings[5]),
                             FCString::Atof(*poseStrings[6]),
                             FCString::Atof(*poseStrings[7]),
                            -FCString::Atof(*poseStrings[4])); // Note: w component is q0 in Cozmo sdk
            zAngleRadians = FCString::Atof(*poseStrings[8]);
            zAngleDegrees = FCString::Atof(*poseStrings[9]);
        }
    }
    
    // Converts pose in Cozmo's coordinate system to Unreal's coordinate system
    FCozmoPoseStruct ToUE4();
    
    UPROPERTY()
    int originID;
    
    UPROPERTY()
    FVector position;
    
    UPROPERTY()
    FQuat rotation;
    
    UPROPERTY()
    float zAngleRadians;
    
    UPROPERTY()
    float zAngleDegrees;
};


UCLASS()
class TREASUREHUNT_API ACozmoUE : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	ACozmoUE();

	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	
    // Called when the game ends
    virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;
    
	// Called every frame
	virtual void Tick(float DeltaSeconds) override;

    // Runs an async function in the Cozmo event loop and optionally invokes C++ callback on main thread upon completion
    // NOTE: coroutine is just whatever string would be typed to create the coroutine from within our CozmoBridge
    // instance.
    // E.g. Pass TEXT("self.sample_coroutine()") to run asynchronous function sample_coroutine of the CozmoBridge
    // class.
    // NOTE: C++ Callback is formatted "FunctionName arg1 arg2 ..."
    void RunCozmoCoroutine(FString coroutine, UObject *doneUObj = NULL, FString doneCall = TEXT(""));
    
    // Check if Cozmo is connected and ready (synchronous)
    bool IsCozmoReady();
    
    // Get Cozmo's current pose
    FCozmoPoseStruct GetCozmoPose();
    
    // Goes to position using go_to_pose, aborting any current actions
    UFUNCTION()
    void ForceGoToPosition(float x, float y, UObject *doneUObj=NULL, FString doneCall=TEXT(""));
    
private:
    
    bool _isCozmoReady = false;
    
    UPythonComponent *_cozmoBridge;
};
