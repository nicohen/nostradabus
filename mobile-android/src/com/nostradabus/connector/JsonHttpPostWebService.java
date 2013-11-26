package com.nostradabus.connector;

import java.io.UnsupportedEncodingException;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.apache.http.NameValuePair;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.methods.HttpRequestBase;
import org.apache.http.entity.ByteArrayEntity;
import org.json.JSONException;
import org.json.JSONObject;

import com.nostradabus.connector.parser.JsonParser;

public class JsonHttpPostWebService extends HttpWebServiceImpl {

	private Map<String, Object> jsonObjects;

	public JsonHttpPostWebService(String urlName,  JsonParser<?> parser) {
		setUrl(urlName);
		setParser(parser);
		jsonObjects = new HashMap<String, Object>();
	}

	public void addParameter(String key, Object object) {
		this.getJsonObjects().put(key, object);
	}

	public JSONObject getWholeObject() {
		JSONObject ret = new JSONObject();
		for (Entry<String, Object> entry : jsonObjects.entrySet()) {
			try {
				ret.accumulate(entry.getKey(), entry.getValue());
			} catch (JSONException e) {
				throw new RuntimeException("Error adding object", e);
			}
		}
		return ret;
	}

	/**
	 * @see com.WebService.connector.HttpWebService#makeHttpMethod()
	 */
	@Override
	protected HttpRequestBase makeHttpMethod() {
		// New post for send request.
		HttpPost post = new HttpPost(getUrl());

		try {
			post.setEntity(new ByteArrayEntity(getWholeObject().toString().getBytes("UTF8")));
		} catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		}

		for (NameValuePair pair : headers) {
			post.addHeader(pair.getName(), pair.getValue());
		}

		return post;
	}

	public Map<String, Object> getJsonObjects() {
		return jsonObjects;
	}
}