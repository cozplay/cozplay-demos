// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "PoseTracker.h"
#include "GameFramework/Actor.h"
#include "LightCubeTracker.generated.h"

// TODO: Give RobotTracker and LightCubeTracker a shared base class
UCLASS()
class TREASUREHUNT_API ALightCubeTracker : public APoseTracker
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	ALightCubeTracker();

	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void Tick( float DeltaSeconds ) override;

protected:
    virtual FCozmoPoseStruct FetchPose() override;
    
private:
    /** Light cube index we should track [0,2] */
    UPROPERTY(EditAnywhere, meta = (ClampMin = "0", ClampMax = "2", UIMin = "0", UIMax = "2"))
    int _index = 0;
};
