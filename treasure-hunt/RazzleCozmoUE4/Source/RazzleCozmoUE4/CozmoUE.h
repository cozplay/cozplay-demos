// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "UnrealEnginePython/Private/UnrealEnginePythonPrivatePCH.h"
#include "UnrealEnginePython/Public/PythonComponent.h"
#include "GameFramework/Actor.h"
#include "CozmoUE.generated.h"

USTRUCT()
struct RAZZLECOZMOUE4_API FCozmoPoseStruct
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
        
//        UE_LOG(LogTemp, Warning, TEXT("OriginID: %d"), originID);
//        UE_LOG(LogTemp, Warning, TEXT("Position: (%f,%f,%f)"), position.X, position.Y, position.Z);
//        UE_LOG(LogTemp, Warning, TEXT("Rotation: (%f,%f,%f,%f)"), rotation.X, rotation.Y, rotation.Z, rotation.W);
//        UE_LOG(LogTemp, Warning, TEXT("ZRad: %f"), zAngleRadians);
//        UE_LOG(LogTemp, Warning, TEXT("ZRad: %f"), zAngleDegrees);
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

// TODO: Better naming consistency Cozmo vs Robot
UCLASS()
class RAZZLECOZMOUE4_API ACozmoUE : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	ACozmoUE();

	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	
    virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;
    
	// Called every frame
	virtual void Tick(float DeltaSeconds) override;

    // Runs an async function in the Cozmo event loop and optionally invokes C++ callback on main thread upon completion
    // NOTE: coroutine is just whatever string would be typed to create the coroutine from within our CozmoBridge
    // instance.
    // E.g. Pass TEXT("self.sample_coroutine1()") to run asynchronous function sample_coroutine1 of the CozmoBridge
    // class.
    // NOTE: C++ Callback is formatted "FunctionName arg1 arg2 ..."
    // TODO: Safer method for passing coroutine argument
    // TODO: Option for return value of coroutine as input to callback
    void RunCozmoCoroutine(FString coroutine, UObject *doneUObj = NULL, FString doneCall = TEXT(""));
    
    // Check if Cozmo is connected and ready (synchronous)
    bool IsCozmoReady();
    
    // Get Cozmo's current pose
    FCozmoPoseStruct GetCozmoPose();
    
    // Returns true if cube with given index is visible
    bool IsCubeVisible(int index);
    
    // Get cube pose
    // Returns null if no cube with index exists
    FCozmoPoseStruct GetCubePose(int index);
    
    UFUNCTION()
    void SampleCallback1();
    
    UFUNCTION()
    void SampleCallback2();
    
    // Goes to position using go_to_pose
    UFUNCTION()
    void ForceGoToPosition(float x, float y, UObject *doneUObj=NULL, FString doneCall=TEXT(""));
    
private:
    UFUNCTION()
    void UpdateCameraFeed();
    
    UPROPERTY(EditAnywhere)
    bool _shouldRunSample = false;
    
    bool _isCozmoReady = false;
    
    /** How many cubes to look for immediately */
    UPROPERTY(EditAnywhere, meta = (ClampMin = "0", ClampMax = "3", UIMin = "0", UIMax = "3"))
    int _initialCubeCheck;
    
    UPROPERTY(EditAnywhere)
    int _initialCubeCheckTimeout = 10;
    
    bool _didInitialCubeCheck = false;
    
    UPROPERTY()
    UTexture2D *cameraFeed;
    
    UPythonComponent *_cozmoBridge;
};
