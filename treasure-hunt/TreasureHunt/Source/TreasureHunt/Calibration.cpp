// Fill out your copyright notice in the Description page of Project Settings.

#include "TreasureHunt.h"
#include "Calibration.h"


// Sets default values for this component's properties
UCalibration::UCalibration()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	bWantsBeginPlay = true;
	PrimaryComponentTick.bCanEverTick = true;

	// ...
}


// Called when the game starts
void UCalibration::BeginPlay()
{
	Super::BeginPlay();

    if (GetOwner()->InputComponent != NULL) {
        BindControls();
        _didBindControls = true;
    }
	
}


// Called every frame
void UCalibration::TickComponent( float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction )
{
	Super::TickComponent( DeltaTime, TickType, ThisTickFunction );

    if (!_didBindControls && GetOwner()->InputComponent != NULL) {
        BindControls();
        _didBindControls = true;
    }
}

void UCalibration::BindControls()
{

}

void UCalibration::LoadGame()
{

}
