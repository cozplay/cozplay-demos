// Fill out your copyright notice in the Description page of Project Settings.

#include "RazzleCozmoUE4.h"
#include "PoseTracker.h"


// Sets default values
APoseTracker::APoseTracker()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void APoseTracker::BeginPlay()
{
	Super::BeginPlay();
	
}

// Called every frame
void APoseTracker::Tick( float DeltaTime )
{
	Super::Tick( DeltaTime );
    FCozmoPoseStruct pose = FetchPose().ToUE4();
    FTransform transform = this->GetTransform();
    
    FVector location = transform.GetLocation();
    if (_trackPosX) {
        location.X = pose.position.X;
    }
    if (_trackPosY) {
        location.Y = pose.position.Y;
    }
    if (_trackPosZ) {
        location.Z = pose.position.Z;
    }
    transform.SetLocation(location);
    
    if (_trackRotation) {
        FQuat rotation = transform.GetRotation();
        rotation.X = pose.rotation.X;
        rotation.Y = pose.rotation.Y;
        rotation.Z = pose.rotation.Z;
        rotation.W = pose.rotation.W;
        transform.SetRotation(rotation);
        SetActorTransform(transform);
    } else if (_trackZAngle) {
        SetActorTransform(transform);
        FRotator rotator = transform.Rotator();
        rotator.Yaw = pose.zAngleDegrees;
        SetActorRelativeRotation(rotator);
    } else {
        SetActorTransform(transform);
    }
}

FCozmoPoseStruct APoseTracker::FetchPose()
{
    FCozmoPoseStruct pose;
    return pose;
}
