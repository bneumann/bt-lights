package bneumann.meisterlampe;

import java.util.ArrayList;
import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.content.pm.PackageManager.NameNotFoundException;
import android.os.Bundle;
import android.os.IBinder;
import android.preference.PreferenceManager;
import android.util.Log;
import android.widget.ArrayAdapter;
import android.widget.ListView;

public class ErrorLogActivity extends Activity
{

	protected static final String TAG = "ErrorLogView";
	private boolean mIsBound = false;
	private String mDefaultDevice;
	private SharedPreferences mSettings;
	private BluetoothService mBluetoothService;
	private ErrorListAdapter mErrorAdapater;
	private ArrayList<String> mInfoList;
	
	private BroadcastReceiver mMessageReceiver = new BroadcastReceiver()
	{
		@Override
		public void onReceive(Context context, Intent intent)
		{
			String action = intent.getAction();
			if (action.equals(BluetoothService.RX_NEW_PACKAGE))
			{
				mErrorAdapater.addAll(mBluetoothService.getLamp().getErrorLog());
			}
		}
	};
	
	
	@Override
	protected void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_error);
		loadSettings();	
		
		mErrorAdapater = new ErrorListAdapter(getApplicationContext());
		ListView lv = (ListView) findViewById(R.id.ErrorList);
		lv.setAdapter(mErrorAdapater);
		
		mInfoList = new ArrayList<String>();
		ArrayAdapter<String> ard = new ArrayAdapter<String>(getApplicationContext(), R.layout.info_list, mInfoList);
		ListView li = (ListView) findViewById(R.id.InfoList);
		li.setAdapter(ard);
		
		registerReceiver(mMessageReceiver, new IntentFilter(BluetoothService.RX_NEW_PACKAGE));
		doBindService();
	}
	
	@Override
	protected void onDestroy()
	{
		super.onDestroy();	
		unregisterReceiver(mMessageReceiver);
		doUnbindService();
		overridePendingTransition(R.anim.fadein, R.anim.fadeout);
	}
	
	@Override
	public void onBackPressed()
	{
		super.onBackPressed();
		doUnbindService();
		overridePendingTransition(android.R.anim.fade_in, android.R.anim.fade_out);
	}
	
	private void fillInfo()
	{
		Lamp lamp = mBluetoothService.getLamp();
		
		mInfoList.add(String.format("App version: %s", getAppVersion()));
		mInfoList.add(String.format("Hardware version: %d, Hardware build: %d", lamp.getVersion(), lamp.getBuild()));
		mInfoList.add(String.format("Commands received: %d", lamp.getCommandCounter()));
		mInfoList.add(String.format("Time on system %02d:%02d:%02d", lamp.getSystemTime()[0],lamp.getSystemTime()[1],lamp.getSystemTime()[2]));		
	}

	private ServiceConnection mServiceConnection = new ServiceConnection()
	{
		

		public void onServiceDisconnected(ComponentName name)
		{
			Log.d(TAG, "Service disconnected");
		}

		public void onServiceConnected(ComponentName name, IBinder service)
		{
			// casting awesomeness to get the service and attach it to the
			mBluetoothService = ((BluetoothService.BluetoothServiceBinder) service).getService();
			Log.d(TAG, "Service connected");	
			mBluetoothService.queryErrorLog();	
			fillInfo();
		}
	};
	
	public String getAppVersion()
	{
		String versionName = "Not Found";
		try
		{
			versionName = getApplicationContext().getPackageManager().getPackageInfo(getApplicationContext().getPackageName(),0).versionName;
		}
		catch (NameNotFoundException e)
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		return versionName;
	}
	
	void doBindService()
	{
		Intent intent = new Intent(this, BluetoothService.class);
		intent.putExtra(BluetoothService.CONNECTION_ADDRESS, this.mDefaultDevice);
		this.mIsBound = bindService(intent, mServiceConnection, BIND_AUTO_CREATE);
	}

	void doUnbindService()
	{
		if(this.mIsBound)
		{
			unbindService(mServiceConnection);
			this.mIsBound = false;
		}
	}
	
	public void loadSettings()
	{
		this.mSettings = PreferenceManager.getDefaultSharedPreferences(this);
		this.mDefaultDevice = mSettings.getString(getString(R.string.pref_default_device), null);
	}
}
