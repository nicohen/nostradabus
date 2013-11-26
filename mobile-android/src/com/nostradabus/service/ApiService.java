package com.nostradabus.service;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import android.util.Log;

import com.nostradabus.application.model.Bus;
import com.nostradabus.application.parser.BusesParser;
import com.nostradabus.application.parser.WebServiceParser;
import com.nostradabus.application.parser.responses.BusesResponse;
import com.nostradabus.connector.HttpGetWebService;
import com.nostradabus.connector.HttpPostWebService;

public class ApiService {

	private final static String TAG = "Nostradabus:ApiService";
	private final static String GET_LINES_ENDPOINT = "GetNearbyLines";
	private final static String POST_CHECKPOINT_ENDPOINT = "DataEntry";

	public static List<Bus> getNearestBuses(double latitude, double longitude) throws Exception {
		Log.i(TAG, "HTTP GET - " + GET_LINES_ENDPOINT);
		HttpGetWebService get = new HttpGetWebService(ConfigurationService.DOMAIN_URL, new BusesParser());
		
		get.setMethod(GET_LINES_ENDPOINT);
		get.addParameter("latitude", String.valueOf(latitude));
		get.addParameter("longitude", String.valueOf(longitude));
		
		BusesResponse busesResponse = (BusesResponse) get.execute();
		if(busesResponse != null) {
			return busesResponse.getBuses();
		}
		
		return new ArrayList<Bus>();
	}
	
	public static void sendCheckpoint(int busLine, double latitude, double longitude, Date locationDate, double speed, int batteryStatus) throws Exception {
		Log.i(TAG, "POST - " + POST_CHECKPOINT_ENDPOINT);
		
		HttpPostWebService get = new HttpPostWebService(ConfigurationService.DOMAIN_URL, new WebServiceParser());
		get.setMethod(POST_CHECKPOINT_ENDPOINT);
		get.addParameter("serialNumber", ConfigurationService.getSimSerialNumber());
		get.addParameter("line", "");
		get.addParameter("latitude", latitude);
		get.addParameter("longitude", longitude);
		DateFormat sdf = new SimpleDateFormat("yyyyMMddHHmmss");
		String formattedDate = sdf.format(locationDate);
		get.addParameter("datetime", sdf.format(locationDate));
		get.addParameter("keycode", ConfigurationService.getSimSerialNumber()+"elgamugol"+formattedDate);
		get.addParameter("speed", speed);
		get.addParameter("batteryStatus", batteryStatus);
		
		get.execute();
	}

}
