package com.nostradabus.connector;

import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map.Entry;

import org.apache.http.NameValuePair;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.methods.HttpRequestBase;
import org.apache.http.message.BasicNameValuePair;

import android.util.Log;

import com.nostradabus.connector.parser.JsonParser;

public class HttpPostWebService extends HttpWebServiceImpl {

	public HttpPostWebService(String urlName,  JsonParser<?> parser) {
		setUrl(urlName);
		setParser(parser);
	}
		
	@Override
	protected HttpRequestBase makeHttpMethod() {
		// New post for send request.
		Log.i("<----HttpPostWebService---->", "url  = " +getUrl());
		HttpPost post = new HttpPost(getUrl());
		
		List<NameValuePair> postParam = new ArrayList<NameValuePair>();
		if (!paramsMap.isEmpty()) {
			for (Entry<String, Object> entry : paramsMap.entrySet()) {
				postParam.add(new BasicNameValuePair(entry.getKey(), (String) entry.getValue()));
			}
		}
		Log.i("<----HttpPostWebService---->", "postWithParameters  = " + postParam);
		
		try {
			post.setEntity(new UrlEncodedFormEntity(postParam));
		} catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		}

		for (NameValuePair pair : headers) {
			post.addHeader(pair.getName(), pair.getValue());
		}

		return post;
	}
}
