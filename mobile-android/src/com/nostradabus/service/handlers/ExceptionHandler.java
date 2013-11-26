package com.nostradabus.service.handlers;

import java.io.IOException;

import org.json.JSONException;

import android.app.Activity;
import android.util.Log;

import com.nostradabus.R;
import com.nostradabus.application.exception.InvalidSessionTokenException;

public class ExceptionHandler {

	/**
	 * This method handles the current thrown exception and constructs a dialog which displays to the user 
	 * the reason of this failure.
	 * 
	 * @param context the context where this should be viewed.
	 * @param e the exception to handle.
	 */
	public static void makeExceptionAlert(final Activity currentActivity, Exception e) {
		String headerText = "";
		String messageText = "";
		Log.e("STExceptionHandler", e.getClass().getName());
		
		if(e instanceof InvalidSessionTokenException) {
			headerText = currentActivity.getText(R.string.STSError).toString();
			messageText = currentActivity.getText(R.string.STSInvalidSessionToken).toString();
		} else if (e instanceof IOException) {
			headerText = currentActivity.getText(R.string.STSError).toString();
			messageText = currentActivity.getText(R.string.STSServersUnreachable).toString();
		} else if (e instanceof JSONException) {
			headerText = currentActivity.getText(R.string.STSError).toString();
			messageText = currentActivity.getText(R.string.STSErrorInvalidServerResponse).toString();
		} else if (e instanceof RuntimeException) {
			headerText = currentActivity.getText(R.string.STSError).toString();
			messageText = currentActivity.getText(R.string.STSErrorUnknown).toString();
		} else {
			headerText = currentActivity.getText(R.string.STSError).toString();
			messageText = currentActivity.getText(R.string.STSErrorUnknown).toString();
		}
		
		final String finalHeaderText = headerText;
		final String finalMessageText = messageText;
		
		currentActivity.runOnUiThread(new Runnable() {
			@Override
			public void run() {
				//showErrorDialog(currentActivity, finalHeaderText, finalMessageText);
			}
		});
	}

	/**
	 * This method throws a new {@link Exception} to handle.
	 * 
	 * @param ex the new exception to handle.
	 * @throws Exception
	 */
	public static void exceptionThrower(Exception ex) throws Exception {
		if (ex != null) {
			if (ex instanceof IOException) {
				throw (IOException) ex;
			} else if (ex instanceof JSONException) {
				throw (JSONException) ex;
			} else if (ex instanceof RuntimeException) {
				throw (RuntimeException) ex;
			} else {
				throw (Exception) ex;
			}
		}
	}
}
