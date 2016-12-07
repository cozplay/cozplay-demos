// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "PoseTracker.h"
#include "GameFramework/Actor.h"
#include "RobotTracker.generated.h"

// TODO: Give RobotTracker and LightCubeTracker a shared base class
//       Move game-specific stuff to subclass (distraction, etc.)
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
    
    UFUNCTION()
    void ShowOutline(bool shouldShow);
    
protected:
    virtual FCozmoPoseStruct FetchPose() override;
    
private:
    UStaticMeshComponent *_outline;
    
    UPROPERTY(EditAnywhere)
    float _distractionTime = 5.0;
};
