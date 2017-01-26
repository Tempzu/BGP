
#include "contiki.h"
#include "lib/random.h"
#include "sys/ctimer.h"
#include "sys/etimer.h"
#include "net/uip.h"
#include "net/uip-ds6.h"
#include "net/uip-debug.h"

#include "sys/node-id.h"

#include "simple-udp.h"
#include "servreg-hack.h"

#include <stdio.h>
#include <string.h>
#include <stdlib.h>

#include "stddef.h" /* For offsetof */
#include "stdio.h"
#include "contiki-conf.h"
#include "net/rime.h"
#include "net/netstack.h"
#include "cc2420.h"
#include "cc2420_const.h"

#include "messages.h"

#define UDP_PORT 1234
#define SERVICE_ID 190

#define SEND_INTERVAL		(15 * CLOCK_SECOND)
#define SEND_TIME		(random_rand() % (SEND_INTERVAL))
#define PRINT_INTERVAL          (3 * 60 * CLOCK_SECOND)

#define LOTTERY_STEP            (CLOCK_SECOND / 20)


static struct simple_udp_connection unicast_connection;
static struct simple_udp_connection unicast_nbr_connection;

enum clusterstates {NOHEAD = 0, LOTTERY = 1, OTHER = 2, THIS = 3};
static enum clusterstates clusterstate;
static uip_ipaddr_t clusterhead_address;

static struct etimer cluster_lottery;
static int was_cluster_head;
/*---------------------------------------------------------------------------*/
PROCESS(unicast_sender_process, "Unicast sender example process");
AUTOSTART_PROCESSES(&unicast_sender_process);
/*---------------------------------------------------------------------------*/
void
print_power()
{
  /* Power consumption calculation variables */
  const static uint32_t power_cpu_01mW = 18 * 3;  // 0.1mW
  const static uint32_t power_lpm_01uW = 545 * 3; // 0.1uW
  const static uint32_t power_tx_01mW = 195 * 3;  // 0.1mW
  const static uint32_t power_rx_01mW = 218 * 3;  // 0.1mW
  uint32_t cpu, lpm, transmit, listen, time, seconds, energy_cpu, energy_lpm, energy_tx, energy_rx, power_microWatts;

  /* Calculate power */
  energest_flush(); // Makes sure all values are updated

  cpu = energest_type_time(ENERGEST_TYPE_CPU);
  lpm = energest_type_time(ENERGEST_TYPE_LPM);
  transmit = energest_type_time(ENERGEST_TYPE_TRANSMIT);
  listen = energest_type_time(ENERGEST_TYPE_LISTEN);
  time = cpu + lpm;

//  seconds = time/RTIMER_SECOND; // Is this needed?
//  energy_cpu = (power_cpu_01mW * ((1000L * cpu)/RTIMER_SECOND))/10000L; //0.1 mW -> mW
//  energy_lpm = (power_lpm_01uW * ((1000L * lpm)/RTIMER_SECOND))/10000L; //0.1uW -> uW
//  energy_rx = (power_rx_01mW * ((1000L * listen)/RTIMER_SECOND))/10000L;
//  energy_tx = (power_tx_01mW * ((1000L * transmit)/RTIMER_SECOND))/10000L;
//  power_microWatts = (energy_cpu*1000L + energy_lpm + energy_rx*1000L + energy_tx*1000L) / seconds; //uJ/s
//  printf("Energy consumption:\nCPU:%lumJ\nLPM:%luuJ=%lu.%03lumJ\nTX:%lumJ\nRX:%lumJ\n",
//          energy_cpu, energy_lpm,
//          energy_lpm / 1000L, energy_lpm - (energy_lpm / 1000L) * 1000L,
//          energy_tx,energy_rx);
//  printf("Average power: %lu.%03lumW\n",
//            power_microWatts / 1000L, power_microWatts - (power_microWatts / 1000L) * 1000L);
  printf("RTIMER_SECOND = %lu\n", RTIMER_SECOND);
  printf("Ticks:\nCPU:%lu\nLPM:%lu\nTX:%lu\nRX:%lu\n",
          cpu,lpm,transmit,listen);
}
/*---------------------------------------------------------------------------*/
uint8_t
get_random_uint8()
{
  // Set the possible output range [0-254] = 255 values
  // 255 reserved for previous clusterheads
  const int range = 255;
  int random;
  random = rand() % range;
  if(random < 0) random = range + random;
  return (uint8_t) random;
}
/*---------------------------------------------------------------------------*/
void
send_data()
{
  uip_ipaddr_t *receiver_address;
  char buf[20];
  uint8_t data = get_random_uint8();

  switch(clusterstate)
  {
    case OTHER:
      receiver_address = &clusterhead_address;
      sprintf(buf, "%s%u", MSG_DATA_PREFIX, data);
      break;
    case THIS:
      receiver_address = servreg_hack_lookup(SERVICE_ID);
      sprintf(buf, "%u", data);
      break;
    default:
      printf("I shouldn't be sending data...\n");
      break;
  }

  if(receiver_address != NULL)
  {
    printf("Sending %s to ", buf);
    uip_debug_ipaddr_print(receiver_address);
    printf("\n");
    simple_udp_sendto(&unicast_connection, buf, strlen(buf) + 1, receiver_address);
  
  } else {
    //printf("Service %d not found\n", SERVICE_ID);
    printf("Receiver address is null!\n");
  }
}
/*---------------------------------------------------------------------------*/
void
adjust_power()
{
  signed char rss;
  signed char rss_val;
  signed char rss_offset;
  rss_val = packetbuf_attr(PACKETBUF_ATTR_RSSI);
  rss_offset=-45;
  rss=rss_val + rss_offset;//[-100,0]

  //printf("RSSI = %d\n",packetbuf_attr(PACKETBUF_ATTR_RSSI)-45); 

  if(rss <= -80)
  {
   printf("RSSI is %d which is <= -80, power set to max\n",rss);
   cc2420_set_txpower(31);
  }
  if(rss <= -70 && rss >=-79)
  {
   printf("RSSI is %d which is [-79,-70], power set to 27\n",rss);
   cc2420_set_txpower(27);
  }
  if(rss <= -60 && rss >=-69)
  {
   printf("RSSI is %d which is [-69,-60], power set to 23\n",rss);
   cc2420_set_txpower(23);
  }
  if(rss <= -50 && rss >=-59)
  {
   printf("RSSI is %d which is [-59,-50], power set to 19\n",rss);
   cc2420_set_txpower(19);
  }
  if(rss <= -40 && rss >=-49)
  {
   printf("RSSI is %d which is [-49,-40], power set to 15\n",rss);
   cc2420_set_txpower(15);
  }
  if(rss <= -30 && rss >=-39)
  {
   printf("RSSI is %d which is [-39,-30], power set to 11\n",rss);
   cc2420_set_txpower(11);
  }
  if(rss <= -20 && rss >=-29)
  {
   printf("RSSI is %d which is [-29,-20], power set to 7\n",rss);
   cc2420_set_txpower(7);
  }
  if(rss <= -10 && rss >=-19)
  {
   printf("RSSI is %d which is [-19,-10], power set to 3\n",rss);
   cc2420_set_txpower(3);
  }
  if(rss <= 0 && rss >=-9)
  {
   printf("RSSI is %d which is [-10,0], power set to 3\n",rss);
   cc2420_set_txpower(3);
  }
}
/*---------------------------------------------------------------------------*/
void
announce_as_clusterhead(void)
{
    uip_ipaddr_t bcast_addr;
    uip_create_linklocal_allnodes_mcast(&bcast_addr);
    simple_udp_sendto(&unicast_nbr_connection, MSG_SET_CLUSTERHEAD, sizeof(MSG_SET_CLUSTERHEAD), &bcast_addr);
}
/*---------------------------------------------------------------------------*/
static void
receiver(struct simple_udp_connection *c,
         const uip_ipaddr_t *sender_addr,
         uint16_t sender_port,
         const uip_ipaddr_t *receiver_addr,
         uint16_t receiver_port,
         const uint8_t *data,
         uint16_t datalen)
{
  char buf[10];
  sprintf(buf, "%s", data);

  printf("FROM: ");
  uip_debug_ipaddr_print(sender_addr);
  printf(" Data '%s' received on port %d from port %d with length %d\n",
         data, receiver_port, sender_port, datalen);
  
  //Other node was faster
  if(strcmp(buf, MSG_SET_CLUSTERHEAD) == 0 && (clusterstate == LOTTERY || clusterstate == NOHEAD))
  {
    clusterstate = OTHER;
    uip_ipaddr_copy(&clusterhead_address, sender_addr);
    printf("Slave to %d", clusterhead_address.u8[15]);
    //uip_debug_ipaddr_print(&clusterhead_address);
    printf("\n");
    adjust_power();
  }
  
  //Reset cluster command from base station
  if(clusterstate == OTHER || clusterstate == THIS)
  {
    if(strcmp(buf, MSG_RESET_CLUSTER) == 0)
    {
      printf("Resetting cluster\n");
      memset(&clusterhead_address, 0, sizeof(clusterhead_address));
      //clusterstate = NOHEAD;
      clusterstate = LOTTERY;
      if(was_cluster_head == 1)
      {
        was_cluster_head = 0;
        etimer_set(&cluster_lottery, LOTTERY_STEP * 255);
      }
      else
      {
        etimer_set(&cluster_lottery, LOTTERY_STEP * get_random_uint8());
      }
    }
  } 
  
  //Forward data to base station
  //printf("D[0]=%c, D[1]=%d D[2]=%d D[3]=%d clusterstate = %d\n",data[0], data[1], data[2], data[3], clusterstate);
  if(data[0] == 'D' && clusterstate == THIS)
  {
    uip_ipaddr_t *receiver_address;
    char buf[datalen]; 
    int iData;
    for(iData = 1;iData < datalen;iData++)
    {
      buf[iData-1] = data[iData];
    }
    buf[iData-1]='\0';
    //printf("buf[0]=%d, buf[1]=%d, buf[2]=%d\n",buf[0], buf[1], buf[2]);
    receiver_address = servreg_hack_lookup(SERVICE_ID);
    
    if(receiver_address != NULL)
    {
      //sprintf(buf, "%u", data[1]);

      printf("Forwarding data '%s' from ", buf);
      uip_debug_ipaddr_print(sender_addr);
      printf(" to ");
      uip_debug_ipaddr_print(receiver_address);
      printf("\n");

      simple_udp_sendto(&unicast_connection, buf, datalen-1, receiver_address);
    }
    else
    {
       printf("Forwarding address is null!\n");
    }
  }
}
/*---------------------------------------------------------------------------*/
static void
set_global_address(void)
{
  uip_ipaddr_t ipaddr;
  int i;
  uint8_t state;

  uip_ip6addr(&ipaddr, 0xaaaa, 0, 0, 0, 0, 0, 0, 0);
  uip_ds6_set_addr_iid(&ipaddr, &uip_lladdr);
  uip_ds6_addr_add(&ipaddr, 0, ADDR_AUTOCONF);

  printf("IPv6 addresses: ");
  for(i = 0; i < UIP_DS6_ADDR_NB; i++) {
    state = uip_ds6_if.addr_list[i].state;
    if(uip_ds6_if.addr_list[i].isused &&
       (state == ADDR_TENTATIVE || state == ADDR_PREFERRED)) {
      uip_debug_ipaddr_print(&uip_ds6_if.addr_list[i].ipaddr);
      printf("\n");
    }
  }
}
/*---------------------------------------------------------------------------*/
PROCESS_THREAD(unicast_sender_process, ev, data)
{
  static struct etimer periodic_timer;
  static struct etimer send_timer;
  static struct etimer print_timer;
  //static struct etimer cluster_lottery;
  //uip_ipaddr_t *addr;
  //uip_ds6_nbr_t *nbr;

  PROCESS_BEGIN();

  clusterstate = NOHEAD;
  was_cluster_head = 0;

  servreg_hack_init();

  set_global_address();

  simple_udp_register(&unicast_connection, UDP_PORT,
                      NULL, UDP_PORT, receiver);
  simple_udp_register(&unicast_nbr_connection, UDP_PORT,
                      NULL, UDP_PORT, receiver);

  etimer_set(&periodic_timer, SEND_INTERVAL);
  etimer_set(&print_timer, PRINT_INTERVAL);
  //etimer_set(&cluster_lottery, LOTTERY_STEP * get_random_uint8());
  while(1) {
    //Get sensor data periodically
    if(etimer_expired(&periodic_timer))
    {
      etimer_reset(&periodic_timer);
      etimer_set(&send_timer, SEND_TIME);
    }
    
    //Print energy consumption
    if(etimer_expired(&print_timer))
    {
      etimer_reset(&print_timer);
      print_power();
    }

    int yieldProcess;
    switch(clusterstate)
    {
      case NOHEAD :
        //printf("No head -> lottery\n");
        etimer_set(&cluster_lottery, LOTTERY_STEP * get_random_uint8());
        clusterstate = LOTTERY;
        yieldProcess = 0;
        break;
      case LOTTERY :
        if(etimer_expired(&cluster_lottery))
        {
          cc2420_set_txpower(31);
          announce_as_clusterhead();
          clusterstate = THIS;
          printf("I am clusterhead\n");
          was_cluster_head = 1;
        }
        yieldProcess = 1;
        break;
      case OTHER :
        if(etimer_expired(&send_timer))
        {
          send_data();
          //adjust_power();
        }
        yieldProcess = 1;
        break;
      case THIS :
        yieldProcess = 1;
        if(etimer_expired(&send_timer))
        {
          cc2420_set_txpower(31);
          send_data();
        }
        break;
      default:
        printf("Something broke\n");
        yieldProcess = 1;
        break;
    }
    if(yieldProcess)
    	PROCESS_YIELD();
  }

  PROCESS_END();
}
/*---------------------------------------------------------------------------*/
