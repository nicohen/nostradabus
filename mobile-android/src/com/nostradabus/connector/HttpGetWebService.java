package com.nostradabus.connector;

import java.util.ArrayList;
import java.util.List;
import java.util.Map.Entry;

import org.apache.http.NameValuePair;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpRequestBase;
import org.apache.http.client.utils.URLEncodedUtils;
import org.apache.http.message.BasicNameValuePair;

import android.util.Log;

import com.nostradabus.connector.parser.JsonParser;

public class HttpGetWebService extends HttpWebServiceImpl {

	public HttpGetWebService(String urlName, JsonParser<?> parser) {
		setUrl(urlName);
		setParser(parser);
	}
	
	@Override
	protected HttpRequestBase makeHttpMethod() {
		
		List<NameValuePair> getParam = new ArrayList<NameValuePair>();
		if (!paramsMap.isEmpty()) {
			for (Entry<String, Object> entry : paramsMap.entrySet()) {
				getParam.add(new BasicNameValuePair(entry.getKey(), (String) entry.getValue()));
			}
		}
		Log.i("<----HttpGetWebService---->", "getWithParameters  = " + getParam);
		
		HttpGet get = new HttpGet(getUrl() + "?" + URLEncodedUtils.format(getParam, "UTF-8"));
		for (NameValuePair pair : headers) {
			get.addHeader(pair.getName(), pair.getValue());
		}

		return get;
	}

}
