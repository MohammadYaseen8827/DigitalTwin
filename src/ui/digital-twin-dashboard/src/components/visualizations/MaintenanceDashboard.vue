<template>
  <div class="maintenance-dashboard p-8">
    <header class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-6 mb-12">
      <div class="flex items-center gap-4">
        <div class="p-3 bg-amber-500/20 rounded-2xl text-amber-500 shadow-lg shadow-amber-500/10">
          <Wrench class="w-6 h-6" />
        </div>
        <div>
          <h2 class="text-2xl font-black tracking-tight text-primary">Maintenance Lifecycle</h2>
          <p class="text-xs font-bold text-secondary uppercase tracking-widest mt-0.5">Fleet Execution Board</p>
        </div>
      </div>
      
      <BaseButton 
        variant="outline" 
        size="sm" 
        class="h-10 px-6 font-bold uppercase tracking-widest text-[10px] border-white/10 hover:bg-white/5"
        @click="loadActiveMaintenance"
      >
        <RefreshCw class="w-4 h-4 mr-2" />
        Sync Board
      </BaseButton>
    </header>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <!-- Planned -->
      <section class="lifecycle-stage min-h-[500px] flex flex-col">
        <div class="flex items-center justify-between mb-6 px-1">
          <h3 class="text-[10px] font-black uppercase tracking-[0.2em] text-secondary flex items-center gap-2">
            <span class="w-2 h-2 rounded-full bg-indigo-500"></span>
            Planned
          </h3>
          <span class="text-[10px] font-bold text-secondary-alt px-2 py-0.5 bg-white/5 rounded-full">{{ plannedRecords.length }}</span>
        </div>
        
        <div class="space-y-4 flex-1">
          <div v-for="record in plannedRecords" :key="record.id" 
               class="p-5 rounded-3xl bg-white/[0.03] border border-white/5 hover:border-indigo-500/30 transition-all group relative overflow-hidden">
            <div class="absolute inset-0 bg-gradient-to-br from-indigo-500/5 to-transparent pointer-events-none"></div>
            
            <div class="flex justify-between items-start mb-4">
              <BaseBadge variant="outline" class="uppercase text-[9px] font-black border-indigo-500/20 text-indigo-400">{{ record.type }}</BaseBadge>
              <span class="text-[10px] font-bold text-secondary-alt">{{ formatDate(record.plannedDate) }}</span>
            </div>
            
            <p class="text-sm font-medium text-secondary line-clamp-2 mb-6 leading-relaxed">{{ record.notes }}</p>
            
            <button @click="startJobAction(record.id)" 
                    class="w-full py-2.5 rounded-xl border border-indigo-500/20 text-indigo-400 text-[10px] font-black uppercase tracking-widest hover:bg-indigo-500 hover:text-white transition-all">
              Initialize Job
            </button>
          </div>
          
          <div v-if="plannedRecords.length === 0" class="py-12 text-center bg-white/[0.02] rounded-3xl border border-dashed border-white/5">
             <Calendar class="w-8 h-8 text-secondary opacity-10 mx-auto mb-3" />
             <p class="text-[10px] font-black uppercase tracking-widest text-secondary-alt">No jobs queued</p>
          </div>
        </div>
      </section>

      <!-- In Progress -->
      <section class="lifecycle-stage min-h-[500px] flex flex-col">
        <div class="flex items-center justify-between mb-6 px-1">
          <h3 class="text-[10px] font-black uppercase tracking-[0.2em] text-amber-500 flex items-center gap-2">
            <span class="w-2 h-2 rounded-full bg-amber-500 animate-pulse"></span>
            In Progress
          </h3>
          <span class="text-[10px] font-bold text-amber-400 px-2 py-0.5 bg-amber-500/10 rounded-full">{{ inProgressRecords.length }}</span>
        </div>
        
        <div class="space-y-4 flex-1">
          <div v-for="record in inProgressRecords" :key="record.id" 
               class="p-5 rounded-3xl bg-amber-500/[0.03] border border-amber-500/10 hover:border-amber-500/30 transition-all group relative overflow-hidden">
            <div class="absolute inset-0 bg-gradient-to-br from-amber-500/5 to-transparent pointer-events-none"></div>
            
            <div class="flex justify-between items-start mb-4">
              <BaseBadge variant="warning" class="uppercase text-[9px] font-black">{{ record.type }}</BaseBadge>
              <span class="text-[10px] font-bold text-amber-400">Since {{ formatTime(record.date) }}</span>
            </div>
            
            <p class="text-sm font-medium text-secondary line-clamp-2 mb-6 leading-relaxed">{{ record.notes }}</p>
            
            <button @click="openCompleteModal(record)" 
                    class="w-full py-2.5 rounded-xl bg-amber-500 text-white text-[10px] font-black uppercase tracking-widest shadow-lg shadow-amber-500/20 hover:scale-[1.02] transition-all">
              Mark Complete
            </button>
          </div>

          <div v-if="inProgressRecords.length === 0" class="py-12 text-center bg-white/[0.02] rounded-3xl border border-dashed border-white/5">
             <Activity class="w-8 h-8 text-secondary opacity-10 mx-auto mb-3" />
             <p class="text-[10px] font-black uppercase tracking-widest text-secondary-alt">No active repairs</p>
          </div>
        </div>
      </section>

      <!-- Recently Completed -->
      <section class="lifecycle-stage min-h-[500px] flex flex-col">
        <div class="flex items-center justify-between mb-6 px-1">
          <h3 class="text-[10px] font-black uppercase tracking-[0.2em] text-emerald-500 flex items-center gap-2">
            <CheckCircle2 class="h-3 w-3" />
            Closed
          </h3>
          <span class="text-[10px] font-bold text-emerald-400 px-2 py-0.5 bg-emerald-500/10 rounded-full font-mono">recent</span>
        </div>
        
        <div class="space-y-4 flex-1">
          <div v-for="record in completedRecords" :key="record.id" 
               class="p-5 rounded-3xl bg-emerald-500/[0.01] border border-white/5 opacity-60 hover:opacity-100 transition-all">
            <div class="flex justify-between items-start mb-3">
              <BaseBadge variant="success" class="uppercase text-[9px] font-black">{{ record.type }}</BaseBadge>
              <span class="text-[10px] font-bold text-secondary-alt">{{ formatDate(record.completionDate) }}</span>
            </div>
            <div class="flex items-center gap-2">
               <div class="w-6 h-6 rounded-full bg-emerald-500/10 flex items-center justify-center text-[10px] font-black text-emerald-400 font-mono">
                  {{ record.performedBy?.charAt(0) ?? 'T' }}
               </div>
               <span class="text-xs font-bold text-secondary">{{ record.performedBy }}</span>
            </div>
          </div>

          <div v-if="completedRecords.length === 0" class="py-12 text-center bg-white/[0.02] rounded-3xl border border-dashed border-white/5">
             <History class="w-8 h-8 text-secondary opacity-10 mx-auto mb-3" />
             <p class="text-[10px] font-black uppercase tracking-widest text-secondary-alt">No history found</p>
          </div>
        </div>
      </section>
    </div>

    <!-- Completion Modal -->
    <Teleport to="body">
      <div v-if="completingRecord" class="fixed inset-0 z-[100] flex items-center justify-center p-6 sm:p-24">
        <div class="absolute inset-0 bg-slate-950/80 backdrop-blur-xl" @click="completingRecord = null"></div>
        
        <div class="relative w-full max-w-lg bg-[#0f172a] rounded-[40px] border border-white/10 shadow-2xl overflow-hidden animate-in zoom-in duration-300">
           <div class="absolute top-0 inset-x-0 h-1 bg-gradient-to-r from-emerald-500 to-indigo-500"></div>
           
           <div class="p-10">
              <h3 class="text-2xl font-black text-primary mb-2">Final Certification</h3>
              <p class="text-secondary text-sm mb-8">Confirm technical completion and logging for Job #{{ completingRecord.id.slice(0, 8) }}</p>
              
              <div class="space-y-6">
                <div class="space-y-2">
                  <label class="text-[10px] font-black text-secondary uppercase tracking-widest pl-1">Authorized Technician</label>
                  <input v-model="completionForm.performedBy" 
                         placeholder="Enter full name"
                         class="w-full bg-white/5 border border-white/10 rounded-2xl py-3 px-4 text-sm font-bold focus:ring-1 focus:ring-emerald-500/50 outline-none transition-all" />
                </div>
                
                <div class="space-y-2">
                  <label class="text-[10px] font-black text-secondary uppercase tracking-widest pl-1">Technical Logs</label>
                  <textarea v-model="completionForm.finalNotes" 
                            rows="4"
                            placeholder="Describe performed actions and parts replaced..."
                            class="w-full bg-white/5 border border-white/10 rounded-2xl py-3 px-4 text-sm font-bold focus:ring-1 focus:ring-emerald-500/50 outline-none transition-all resize-none"></textarea>
                </div>
              </div>
              
              <div class="flex gap-4 mt-10">
                <BaseButton variant="ghost" class="flex-1 h-12 font-black uppercase text-[11px]" @click="completingRecord = null">Abort</BaseButton>
                <BaseButton variant="primary" class="flex-[1.5] h-12 font-black uppercase text-[11px] shadow-lg shadow-emerald-500/20" @click="handleComplete">Certify Completion</BaseButton>
              </div>
           </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useMaintenanceStore } from '@/stores/maintenance.store'
import { useToast } from '@/composables/useToast'
import BaseButton from '../base/BaseButton.vue'
import BaseBadge from '../base/BaseBadge.vue'
import { Wrench, RefreshCw, Calendar, Activity, CheckCircle2, History } from 'lucide-vue-next'

const maintenanceStore = useMaintenanceStore()
const { records } = storeToRefs(maintenanceStore)
const { loadActiveMaintenance, startJob, completeJob } = maintenanceStore
const toast = useToast()

const completingRecord = ref<any>(null)
const completionForm = ref({ performedBy: '', finalNotes: '' })

const plannedRecords = computed(() => records.value.filter(r => r.status === 'Planned'))
const inProgressRecords = computed(() => records.value.filter(r => r.status === 'InProgress'))
const completedRecords = computed(() => records.value.filter(r => r.status === 'Completed').slice(0, 5))

function formatDate(dateString?: string) {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

function formatTime(dateString?: string) {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

async function startJobAction(id: string) {
  try {
    await startJob(id)
    toast.success('Maintenance job initialized')
    loadActiveMaintenance()
  } catch (err) {
    toast.error('Failed to start job')
  }
}

function openCompleteModal(record: any) {
  completingRecord.value = record
  completionForm.value = { performedBy: '', finalNotes: '' }
}

async function handleComplete() {
  if (!completingRecord.value) return
  if (!completionForm.value.performedBy) {
    toast.warning('Authorized technician name required')
    return
  }
  
  try {
    await completeJob(completingRecord.value.id, completionForm.value)
    completingRecord.value = null
    loadActiveMaintenance()
    toast.success('Work certified and closed')
  } catch (err) {
    toast.error('Sync error during certification')
  }
}

onMounted(() => {
  loadActiveMaintenance()
})
</script>

<style scoped>
.maintenance-dashboard {
  max-width: 1400px;
  margin: 0 auto;
}

.lifecycle-stage {
  border-radius: 40px;
  background: rgba(15, 23, 42, 0.1);
  padding: 8px;
}
</style>

