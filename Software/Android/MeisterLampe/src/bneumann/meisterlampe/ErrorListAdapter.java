package bneumann.meisterlampe;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.Locale;
import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;
import bneumann.protocol.ErrorEntry;
import bneumann.protocol.Frame;

public class ErrorListAdapter extends ArrayAdapter<Frame>
{
	private static final String TAG = "ErrorLogAdapter";
	private ArrayList<String> mErrorTable = new ArrayList<String>();
	private Context mContext;
	
	public ErrorListAdapter(Context context)
	{
		super(context, R.layout.list);
		this.mContext = context;
		mErrorTable.add(0x00, "Just unknown command (maybe came through checksum by incident)");
		mErrorTable.add(0x01, "Header not found, clearing memory");
		mErrorTable.add(0x02, "This function is not declared");
		mErrorTable.add(0x03, "A channel changed its value very fast, this should not happen accidently");
		mErrorTable.add(0x04, "The write or read buffer pointer are out of range");
		mErrorTable.add(0x05, "Wrong mode set. Will be set to NOOP instead");
		mErrorTable.add(0x06, "A port that was off the scope has been addressed");
		mErrorTable.add(0x07, "A value has been set that caused a driver error");
	}
	
	public void changeContext(Context context)
	{
		this.mContext = context;
	}
	
	public void addAll(ArrayList<Frame> frames)
	{
		Iterator<Frame> it = frames.iterator();
		while(it.hasNext())
		{
			Frame curFrame = it.next();
			if(!this.contains(curFrame))
			{
				this.add(curFrame);
			}
		}
	}
	
	public boolean contains(Frame frame)
	{
		for (int i = 0; i < this.getCount(); i++)
		{
			if (this.getItem(i).getPayload() == frame.getPayload() && this.getItem(i).getFunction() == frame.getFunction())
			{
				return true;
			}
		}
		return false;
	}
	
	@Override
	public View getView(int position, View convertView, ViewGroup parent)
	{
		LayoutInflater inflater = (LayoutInflater) this.mContext.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
		View rowView = inflater.inflate(R.layout.list, parent, false);

		Frame it = getItem(position);
		ErrorEntry curFrame = new ErrorEntry();				
		try
		{
			curFrame = new ErrorEntry(it.getByteData());
		}
		catch (Exception e)
		{
			Log.e(TAG, "Some entries have not been added to error list");
			e.printStackTrace();
		}
		String entry = mErrorTable.get(curFrame.getError());
		int[] ts = Lamp.getTimeArray(curFrame.getTimeStamp());
		String timeStamp = String.format(Locale.getDefault(), "[%02d:%02d:%02d]", ts[0], ts[1], ts[2]);
		
		TextView textViewFirst = (TextView) rowView.findViewById(R.id.ListFirstLine);
		textViewFirst.setText(entry);
		TextView textViewSecond = (TextView) rowView.findViewById(R.id.ListSecondLine);
		textViewSecond.setText(timeStamp);

		return rowView;
	}
}
