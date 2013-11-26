package com.nostradabus.application.model;

import java.text.SimpleDateFormat;
import java.util.Calendar;

import org.json.JSONException;
import org.json.JSONObject;

import com.nostradabus.application.util.DateUtil;

public class CurrentPosition {
	private Long id;
	private double latitude;
	private double longitude;
	private String date;
	
	public CurrentPosition(double latitude, double longitude) {
		this.latitude = latitude;
		this.longitude = longitude;
		SimpleDateFormat sdf = new SimpleDateFormat(DateUtil.DATE_TIME_FORMAT);
		this.date = sdf.format(Calendar.getInstance().getTime());
	}
	
	public CurrentPosition(double latitude, double longitude, String date) {
		this.latitude = latitude;
		this.longitude = longitude;
		this.date = date;
	}

	public Long getId() {
		return id;
	}

	public void setId(Long id) {
		this.id = id;
	}

	public double getLatitude() {
		return latitude;
	}

	public void setLatitude(double latitude) {
		this.latitude = latitude;
	}

	public double getLongitude() {
		return longitude;
	}

	public void setLongitude(double longitude) {
		this.longitude = longitude;
	}

	public String getDate() {
		return date;
	}

	public void setDate(String date) {
		this.date = date;
	}
	
	public JSONObject toJson() throws JSONException {
		JSONObject res = new JSONObject();
		
		res.accumulate("Latitude", this.latitude);
		res.accumulate("Longitude", this.longitude);
		res.accumulate("Date", this.date);
		
		return res;
	}
}
