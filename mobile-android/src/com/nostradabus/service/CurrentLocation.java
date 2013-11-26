package com.nostradabus.service;

import java.util.Date;

import android.database.DataSetObservable;
import android.location.Location;

import com.google.android.maps.GeoPoint;

public class CurrentLocation extends DataSetObservable {

	private static CurrentLocation _instance = new CurrentLocation();
    private Date lastUpdateUserLog;
	private Location location;
	private boolean gpsLocationForced;
	private Date lastGpsLocationForced;

	private CurrentLocation() {
		location = new Location("");
	}

	public static CurrentLocation getInstance() {
		return _instance != null ? _instance : new CurrentLocation();
	}

	public Location getLocation() {
		return location;
	}

	public void update(Location location) {
		this.location = location; 
		
		notifyChanged();
	}

	public GeoPoint getUserGeoPoint() {
		if (null != location) {
			GeoPoint geoPoint = this.getGeoPoint(
					location.getLatitude(), location.getLongitude()
			);
			return geoPoint;
		}

		return null;
	}
	
	public GeoPoint getGeoPoint(double latitude, double longitude) {
		 return new GeoPoint(Double.valueOf(latitude*1E6).intValue(), Double.valueOf(longitude*1E6).intValue());
	 }

	public void setLastUpdateUserLog(Date lastUpdateUserLog) {
		this.lastUpdateUserLog = lastUpdateUserLog;
	}

	public Date getLastUpdateUserLog() {
		return lastUpdateUserLog;
	}

	public boolean isGpsLocationForced() {
		return gpsLocationForced;
	}

	public void setGpsLocationForced(boolean gpsLocationForced) {
		this.gpsLocationForced = gpsLocationForced;
	}

	public Date getLastGpsLocationForced() {
		return lastGpsLocationForced;
	}

	public void setLastGpsLocationForced(Date lastGpsLocationForced) {
		this.lastGpsLocationForced = lastGpsLocationForced;
	}

}