// Fill out your copyright notice in the Description page of Project Settings.

#include "TreasureHunt.h"
#include "MoveUpAndDown.h"


// Sets default values for this component's properties
UMoveUpAndDown::UMoveUpAndDown()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	bWantsBeginPlay = true;
	PrimaryComponentTick.bCanEverTick = true;

	// ...
}


// Called when the game starts
void UMoveUpAndDown::BeginPlay()
{
	Super::BeginPlay();

    _initialZ = GetOwner()->GetActorLocation().Z;
}


// Called every frame
void UMoveUpAndDown::TickComponent( float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction )
{
	Super::TickComponent( DeltaTime, TickType, ThisTickFunction );
    _elapsed = FMath::Fmod(_elapsed + DeltaTime, 2.0f*PI);
    FVector location = GetOwner()->GetActorLocation();
    location.Z = _initialZ + FMath::Sin(_elapsed) * _amplitude;
    GetOwner()->SetActorLocation(location);
}

