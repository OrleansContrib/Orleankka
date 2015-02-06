Domain
------
The application provides users with ability to continuosly monitor various social networks.
Users create topics of interest, which then used in queries to a particular vendor search APIs.

Requirements
------------
1. Reliable searching

   Search API could fail and be temporariliy unavailable for some time.
   It could be an intermittent connection failure or it could be a serious outage.
   Intermittent failures should be gracefully handled without introducing unneccessary delays.
   In case of serious problems (API outages) human intervention is ok.
   
2. Economically scalable

    As the number of users increase, the per-user cost of the 
	overall system must decrease in order to sustain the system.
		
3. Fair use of resources

   Certain social networks have certain limits for search requests per-day (hour, second).
   That said, the solution should take this into account and provide fair search request
   distribution across all users (topics).
   
4. Designed for availability

   The application should aim for high availability while meeting 
   defined performance targets (sub-second response time)

NOTE: "Fair use of resources" is out of scope for the sample project


Actors
------
Api is the singleton actor per search provider (social network)

	- Api performs search requests against certain search provider APIs
	
	- Api implements CircuitBreaker pattern:

		- Api could lock itself and won't issue "real" search requests 
		  until certain search provider API is available again

		- Api could notify when availability changes

	- Api manages search provider's request limits (*not in the sample project*)


Topic executes user specified queries against particular Apis on a recurrent schedule
	
	- Topic aggregates results received from Api
	- Topic stores aggregated total in a BLOB
	- Topic stores configured schedule in another BLOB
	- Topic restores its schedule upon activation, ie updates persistent reminders

SystemConsole is an external client which monitors availability of Api
	- Subscribes to notifications from Api
	- Administrators could use it to re-enable searches for particular Api
	

NOTE: Store/restore of Topic's schedule is out of scope for the sample project.


Scenarios (in progress)
-----------------------
First time Topic get failed reply from Api, 
it should schedule local timer to retry request every 5 seconds
		
	If Api is still unavailable for 3 consecutive retries:
		Topic should disable search for that Api (ie, delete persistent reminder)
		Topic should cancel local retry timer

	If during retries Api is back avaiable again
		Topic should cancel local retry timer
		Topic should allow scheduled searches
		    
	If Topic receives scheduled search request (from persistent reminder)
		While in retry state - it should ignore it
		Otherwise it should issue search request to particular Api								