// Fill out your copyright notice in the Description page of Project Settings.

#include "TreasureHunt.h"
#include "LightCubeTracker.h"


// Sets default values
ALightCubeTracker::ALightCubeTracker()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ALightCubeTracker::BeginPlay()
{
	Super::BeginPlay();
	
}

// Called every frame
void ALightCubeTracker::Tick( float DeltaTime )
{
	Super::Tick( DeltaTime );

}

FCozmoPoseStruct ALightCubeTracker::FetchPose()
{
    return _cozmoUE->GetCubePose(_index);
}
