package com.nostradabus.connector.parser;

import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class JsonSerializer {

	@SuppressWarnings("unchecked")
	public static String serialize(Map<String, Object> paramList) {
		JSONObject json = new JSONObject();
		
		try {
			for (Entry<String, Object> entry : paramList.entrySet()) {
				if (entry.getValue() instanceof List) {
					json.put(entry.getKey(), new JSONArray((List<Object>) entry.getValue()));
				} else if (entry.getValue() instanceof Map) {
					json.put(entry.getKey(), new JSONObject((Map<String, Object>) entry.getValue()));
				} else {
					json.put(entry.getKey(), entry.getValue());
				}
			}
		} catch (JSONException e) {
			throw new RuntimeException(e);
		}
		
		return json.toString();
	}
	
	@SuppressWarnings("unchecked")
	public static String serialize(List<Object> paramList) {
		JSONArray jsonArray = new JSONArray();
		
		for (Object value : paramList) {
			if (value instanceof List) {
				jsonArray.put(new JSONArray((List<Object>) value));
			} else if (value instanceof Map) {
				jsonArray.put(new JSONObject((Map<String, Object>) value));
			} else {
				jsonArray.put(value);
			}
		}
		
		return jsonArray.toString();
	}
	
}
