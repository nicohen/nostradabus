package com.nostradabus.service;

import android.content.Context;
import android.location.Criteria;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;

public class LocationService {
	
	private static LocationListener locationListenerGps = new LocationListener() {
		public void onLocationChanged(Location location) {
//			if (location.getAccuracy() <= ConfigurationService.MINIMUM_GPS_ACCURACY) {
				CurrentLocation.getInstance().update(location);
				
//				CurrentPosition position = new CurrentPosition(location.getLatitude(), location.getLongitude());
		        
		        //UpdateUserLogHelper.updateUserLog(position);
//			}
		}

		public void onStatusChanged(String provider, int status, Bundle extras) {}

		public void onProviderEnabled(String provider) {}

		public void onProviderDisabled(String provider) {}
	};

	private static LocationListener locationListenerNetwork = new LocationListener() {
		public void onLocationChanged(Location location) {
			CurrentLocation.getInstance().update(location);
		}

		public void onStatusChanged(String provider, int status, Bundle extras) {}

		public void onProviderEnabled(String provider) {}

		public void onProviderDisabled(String provider) {}
	};
	
	public static boolean hasConnectivity(Context context, LocationManager locationManager) {
		return servicesActive(locationManager) &&
				hasNetworkConnectivity(context);
	}
	
	public static boolean servicesActive(LocationManager locationManager) {
		return locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER) ||
				locationManager.isProviderEnabled(LocationManager.NETWORK_PROVIDER);
	}
	
	public static boolean hasNetworkConnectivity(Context context) {
//        ConnectivityManager connManager = (ConnectivityManager) context.getSystemService(Context.CONNECTIVITY_SERVICE);
//        NetworkInfo mWifi = connManager.getNetworkInfo(ConnectivityManager.TYPE_WIFI);
        
//        return mWifi.isAvailable() && 
//        return Settings.System.getInt(context.getContentResolver(), Settings.System.AIRPLANE_MODE_ON, 0) == 0;
		return true;
    }
	
	/**
	 * Stop the location provider
	 * 
	 * @param provider
	 */
	public static void stopProvider(LocationManager locationManager, String provider) {

		if (LocationManager.GPS_PROVIDER.equals(provider)) {
			locationManager.removeUpdates(locationListenerGps);

		} else if (LocationManager.NETWORK_PROVIDER.equals(provider)) {
			locationManager.removeUpdates(locationListenerNetwork);
		}
	}

	public static void startProvider(LocationManager locationManager) {
		Criteria criteria = new Criteria();
        criteria.setAccuracy(Criteria.ACCURACY_FINE);
        criteria.setAltitudeRequired(false);
        criteria.setBearingRequired(false);
        criteria.setCostAllowed(false);
        criteria.setSpeedRequired(true);
		String provider = locationManager.getBestProvider(criteria, true);
		
		final boolean gpsEnabled = locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER);

		if (gpsEnabled && LocationManager.GPS_PROVIDER.equals(provider)) {
			locationManager.requestLocationUpdates(provider, 15000, 0, locationListenerGps); 
		} else if (LocationManager.NETWORK_PROVIDER.equals(provider)) {
			locationManager.requestLocationUpdates(provider, 15000, 0, locationListenerNetwork); 
		}
	}
    
}
