package com.nostradabus.connector;

import java.io.UnsupportedEncodingException;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.apache.http.NameValuePair;
import org.apache.http.client.methods.HttpPut;
import org.apache.http.client.methods.HttpRequestBase;
import org.apache.http.entity.ByteArrayEntity;
import org.json.JSONException;
import org.json.JSONObject;

import com.nostradabus.connector.parser.JsonParser;

public class JsonHttpPutWebService extends HttpWebServiceImpl {

	private Map<String, JSONObject> jsonObjects;
	
	public JsonHttpPutWebService(String urlName,  JsonParser<?> parser) {
		setUrl(urlName);
		setParser(parser);
		jsonObjects = new HashMap<String, JSONObject>();
	}
	
	public void addParameter(String key, JSONObject object) {
		this.getJsonObjects().put(key, object);
	}
	
	public JSONObject getWholeObject() {
		JSONObject ret = new JSONObject();
		for (Entry<String, JSONObject> entry : jsonObjects.entrySet()) {
			try {
				ret.accumulate(entry.getKey(), entry.getValue());
			} catch (JSONException e) {
				throw new RuntimeException("Error adding object", e);
			}
		}
		return ret;
	}
	
	@Override
	public void addParameter(String name, Object value) {
		throw new RuntimeException("Method overriden");		
	};
	
	/**
	 * @see com.WebService.connector.HttpWebService#makeHttpMethod()
	 */
	@Override
	protected HttpRequestBase makeHttpMethod() {
		// New put to send request.
		HttpPut put = new HttpPut(getUrl());
		
		try {
			put.setEntity(new ByteArrayEntity(getWholeObject().toString().getBytes("UTF8")));
		} catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		}

		for (NameValuePair pair : headers) {
			put.addHeader(pair.getName(), pair.getValue());
		}

		return put;
	}

	public Map<String, JSONObject> getJsonObjects() {
		return jsonObjects;
	}
}
