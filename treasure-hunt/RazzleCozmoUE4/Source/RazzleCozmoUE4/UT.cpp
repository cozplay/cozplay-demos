// Fill out your copyright notice in the Description page of Project Settings.

#include "RazzleCozmoUE4.h"
#include "UT.h"

//Trace using start point, direction, and length
bool UT::Trace(UWorld* world, AActor* actorToIgnore, const FVector& start, const FVector& dir, float length,
               FHitResult& hit, ECollisionChannel collisionChannel, bool returnPhysMat)
{
    return Trace(world, actorToIgnore, start, start + dir*length, hit, collisionChannel, returnPhysMat);
}

// Trace using start point, and end point
bool UT::Trace(UWorld* world, AActor* actorToIgnore, const FVector& start, const FVector& end, FHitResult& hit,
               ECollisionChannel collisionChannel, bool returnPhysMat)
{
    if (!world)
    {
        return false;
    }
    
    // Trace params, set the 'false' to 'true' if you want it to trace against the actual meshes instead of their
    // collision boxes.
    FCollisionQueryParams TraceParams(FName(TEXT("VictoreCore Trace")), false, actorToIgnore);
    TraceParams.bReturnPhysicalMaterial = returnPhysMat;
    
    //Ignore Actors, usually the actor that is calling the trace
    TraceParams.AddIgnoredActor(actorToIgnore);
    
    //Re-initialize hit info, so you can call the function repeatedly and hit will always be fresh
    hit = FHitResult(ForceInit);
    
    //Trace!
    bool hitSomething = world->LineTraceSingleByChannel(
                                                        hit,		//result
                                                        start,	//start
                                                        end, //end
                                                        collisionChannel, //collision channel
                                                        TraceParams
                                                        );
    
    // Draw a square at the impact point.
    if (hitSomething) DrawDebugPoint(world, hit.ImpactPoint, 10, FColor(255, 255, 0), false, -1);
    
    // Draw the trace line. Red if something was hit, green if nothing was hit.
    DrawDebugLine(world, start, end, (hitSomething ? FColor(255, 0, 0) : FColor(0, 255, 0)), false, -1, 0, 1.5);
    
    return hitSomething;
}
