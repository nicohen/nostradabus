package com.nostradabus.application.parser;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import com.nostradabus.application.model.Bus;
import com.nostradabus.application.parser.responses.BusesResponse;
import com.nostradabus.connector.parser.JsonParser;

public class BusesParser extends JsonParser<BusesResponse> {

	@Override
	protected BusesResponse parse(Object json) throws JSONException {
		BusesResponse busesResponse = new BusesResponse();
		boolean success = false;
		
		JSONObject jsonObject = (JSONObject) json;

		if (!jsonObject.isNull("message")) {
			busesResponse.setMessage(jsonObject.getString("message"));
		}
		if (!jsonObject.isNull("ResponseCode")) {
			busesResponse.setResponseCode(jsonObject.getInt("ResponseCode"));
		}
		if (!jsonObject.isNull("success")) {
			busesResponse.setSuccess(jsonObject.getBoolean("success"));
		}

		if(!jsonObject.isNull("success")) {
			success = jsonObject.getBoolean("success");
			if(success) {
				if(!jsonObject.isNull("lines")) {
					JSONArray lines = jsonObject.getJSONArray("lines");
					for(int i=0; i<lines.length(); i++) {
						Bus bus = new Bus();
						bus.setNumero(lines.getInt(i));
						busesResponse.addBus(bus);
					}
				}
			}
			return busesResponse;
		}
		
		return null;
	}
}