package com.nostradabus.service;

import android.app.Application;
import android.content.SharedPreferences;

import com.nostradabus.application.NostradabusApplication;

public class PersistenceService {
	
	private static final String SESSION_TOKEN = "session_token";
	private static final String STAY_LOGGED_IN = "stay_logged_in";
	
	/**
	 * Stores the given value in the SharedPreferences, under the given key. 
	 * 
	 * @param key The key to identify this value.
	 * @param value the string to store.
	 */
	private static synchronized final void persist(String key, String value) {
		SharedPreferences settings = getSharedPreferences();
		SharedPreferences.Editor editor = settings.edit();
		editor.putString(key, value);
		editor.commit();
	}
	
	/**
	 * Stores the given value in the SharedPreferences, under the given key. 
	 * 
	 * @param key The key to identify this value.
	 * @param value the boolean to store.
	 */
	private static synchronized final void persist(String key, Boolean value) {
		SharedPreferences settings = getSharedPreferences();
		SharedPreferences.Editor editor = settings.edit();
		editor.putBoolean(key, value);
		editor.commit();
	}

	private static synchronized final void removePersisted(String key) {
		SharedPreferences settings = getSharedPreferences();
		SharedPreferences.Editor editor = settings.edit();
		editor.remove(key);
		editor.commit();
	}

	/**
	 * Retrieves the value stored under the given key.
	 * 
	 * @param key string to search for in the SharedPreferences.
	 * 
	 * @return the value if any, an empty string otherwise. 
	 */
	private static final String getPersisted(String key) {
		return getSharedPreferences().getString(key, "");
	}
	
	/**
	 * Retrieves the value stored under the given key.
	 * @param key string to search for in the SharedPreferences.
	 * @return the value if any, false otherwise. 
	 */
	private static final Boolean getPersistedBoolean(String key) {
		return getSharedPreferences().getBoolean(key, false);
	}
	
	/**
	 * This method retrieves the stored SharedPreferences from the application.
	 * 
	 * @return the {@link SharedPreferences} object.
	 */
	private static final SharedPreferences getSharedPreferences() {
		return NostradabusApplication.get().getSharedPreferences("ColectivoApplication", Application.MODE_PRIVATE);
	}
	
	public static void persistSessionToken(String sessionToken) {
		persist(SESSION_TOKEN, sessionToken);
	}
	
	public static final String getPersistedSessionToken() {
		return getPersisted(SESSION_TOKEN);
	}
	
	public static void removePersistedSessionToken() {
		removePersisted(SESSION_TOKEN);
		setStayLogged(false);
	}
	
	public static final void setStayLogged(boolean stayLogged) {
		persist(STAY_LOGGED_IN, stayLogged);
	}

	public static final boolean stayLogged() {
		return getPersistedBoolean(STAY_LOGGED_IN);
	}
	
}
