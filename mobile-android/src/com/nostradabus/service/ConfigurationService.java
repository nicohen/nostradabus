/**
 * ConfigurationService.java
 *
 * @copyright 2011 Monits
 * @license Copyright (C) 2011. All rights reserved
 * @version Release: 1.0.0
 * @link http://www.monits.com/
 * @since 1.0.0
 */
package com.nostradabus.service;

import android.content.Context;
import android.telephony.TelephonyManager;

import com.nostradabus.application.NostradabusApplication;

public class ConfigurationService {
	public static final int POSITION_NOTIFY_INTERVAL = 5 * 1000 * 60;
	public static final int POSITION_UPDATE_INTERVAL = 4 * 1000 * 60;
	
	public static final int MINIMUM_TIME_TO_ALLOW_EMERGENCY_CHECKIN = 420; //In seconds
	public static final int MINIMUM_TIME_TO_FIND_GEOLOCATION = 40; //In seconds
	public static final int MINIMUM_DISTANCE_TO_ALLOW_CHECKIN = 500; //In meters
	
	public static final int MINIMUM_GPS_ACCURACY = 10; //In meters
	
	public static final int FORCED_GPS_FIX_EXPIRE = 300; //In seconds
	
	public static final String DOMAIN_URL = "http://nostradabus.elasticbeanstalk.com/Mobile";
	
	public static String getSimSerialNumber() { 
		TelephonyManager tMgr = (TelephonyManager) NostradabusApplication.get().getSystemService(Context.TELEPHONY_SERVICE);
		return tMgr.getSimSerialNumber();
	}

}
