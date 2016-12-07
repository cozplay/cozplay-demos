// Fill out your copyright notice in the Description page of Project Settings.

#include "RazzleCozmoUE4.h"
#include "RobotTracker.h"


// Sets default values
ARobotTracker::ARobotTracker()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ARobotTracker::BeginPlay()
{
	Super::BeginPlay();
    _outline = (UStaticMeshComponent *)GetComponentsByTag(UStaticMeshComponent::StaticClass(), TEXT("Outline"))[0];
    ShowOutline(false);
}

// Called every frame
void ARobotTracker::Tick( float DeltaTime )
{
	Super::Tick( DeltaTime );
}

void ARobotTracker::ShowOutline(bool shouldShow)
{
    if (_outline) {
        _outline->SetHiddenInGame(!shouldShow);
    }
}

FCozmoPoseStruct ARobotTracker::FetchPose()
{
    return _cozmoUE->GetCozmoPose();
}
