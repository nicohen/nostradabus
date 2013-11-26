package com.nostradabus.connector.parser;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.util.Log;

public abstract class JsonParser<E extends ParseResult> implements WebServiceParser<E> {
	
	/**
	 * 
	 * @param inputStream
	 * @return
	 */
	public E parse(InputStream inputStream) {
		try {
			// A Simple JSON Response Read
			final String result = convertStreamToString(inputStream);
			
			Log.d("JsonParser", result);
			if (result.length() == 0) {
				//TODO: throw new exception
				throw new RuntimeException();
			}
			// A Simple JSONObject Creation
			final Object json = makeJsonElement(result);
			// A Simple JSONObject Parsing
			return parse(json);
		}
		catch (JSONException e) {
			//TODO: throw new exception
			e.printStackTrace();
			throw new RuntimeException();
		}
		finally {
		}
	}
	
	/**
	 * @param json
	 * @return
	 */
	protected abstract E parse(Object json) throws JSONException;

	/**
	 * @param result
	 * @return
	 * @throws JSONException
	 */
	protected Object makeJsonElement(String result) throws JSONException {
		if (result.startsWith("[")) {
			return new JSONArray(result);
		} else {
			return new JSONObject(result);
		}
	}

	private String convertStreamToString(InputStream is) {
		BufferedReader reader = new BufferedReader(new InputStreamReader(is));
		StringBuilder sb = new StringBuilder();

		try {
			int read;
			
			while ((read = reader.read()) != -1) {
				sb.append((char) read);
			}
		}
		catch (IOException e) {
			e.printStackTrace();
		}
		finally {
			try {
				is.close();
			}
			catch (IOException e) {
				e.printStackTrace();
			}
		}
		return sb.toString();
	}


}
