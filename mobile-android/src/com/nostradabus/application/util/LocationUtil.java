package com.nostradabus.application.util;

import android.location.Location;

public class LocationUtil {
	public static double GetDistance(Location sourceLocation, Location destinationLocation) {
        double distance =  (3956 * 2 * Math.asin(Math.sqrt(Math.pow(Math.sin((sourceLocation.getLatitude() - destinationLocation.getLatitude()) * Math.PI/180 / 2), 2) +
               Math.cos(sourceLocation.getLatitude() * Math.PI/180) * Math.cos(sourceLocation.getLatitude() * Math.PI/180) *
                 Math.pow(Math.sin((sourceLocation.getLongitude() - destinationLocation.getLongitude()) * Math.PI/180 / 2), 2))))*1.609344;

        return distance;
    }
}
